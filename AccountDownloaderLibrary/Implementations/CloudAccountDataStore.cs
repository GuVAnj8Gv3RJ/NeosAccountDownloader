using AccountDownloaderLibrary.Implementations;
using AccountDownloaderLibrary.Mime;
using AccountDownloaderLibrary.Models;
using CloudX.Shared;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AccountDownloaderLibrary
{
    public class ExtensionResult : IExtensionResult
    {
        public string Extension { get; set; }

        public string MimeType { get; set; }
    }
    public class CloudAccountDataStore : IAccountDataGatherer
    {
        public readonly CloudXInterface Cloud;

        public string Name => Cloud.UserAgentProduct + " " + Cloud.UserAgentVersion;
        public string UserId => Cloud.CurrentUser.Id;
        public string Username => Cloud.CurrentUser.Username;

        private readonly ILogger Logger;

#pragma warning disable CS0067 // The event 'CloudAccountDataStore.ProgressMessage' is never used
        public event Action<string> ProgressMessage;
#pragma warning restore CS0067 // The event 'CloudAccountDataStore.ProgressMessage' is never used

        public int FetchedGroupCount { get; private set; }

        public static readonly DateTime EARLIEST_API_TIME = new(2016, 1, 1);

        private CancellationToken CancelToken;

        private const string DB_PREFIX = "neosdb:///";
        private readonly HttpClient WebClient = new();

        private readonly AccountDownloadConfig Config;

        public CloudAccountDataStore(CloudXInterface cloud, AccountDownloadConfig config, ILogger logger)
        {
            this.Cloud = cloud;
            this.Config = config;

            Logger = logger;

            //TODO: this just doubles the default timeout of 100s. 
            WebClient.Timeout = TimeSpan.FromSeconds(200);
        }

        public virtual async Task Prepare(CancellationToken token)
        {
            CancelToken = token;

            await Cloud.UpdateCurrentUserMemberships().ConfigureAwait(false);

            FetchedGroupCount = Cloud.CurrentUserGroupInfos.Where(g => g.AdminUserId == Cloud.CurrentUser.Id).Count();
        }

        public virtual async Task Complete()
        {
            await Task.CompletedTask;
        }

        public virtual async Task<List<Friend>> GetContacts()
        {
            CloudResult<List<Friend>> result = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                result = await Cloud.GetFriends().ConfigureAwait(false);

                if (result.IsOK)
                    return result.Entity;

                await Task.Delay(TimeSpan.FromSeconds(attempt * 1.5), CancelToken).ConfigureAwait(false);
            }

            throw new Exception("Could not fetch contacts after several attempts. Result: " + result);
        }

        public virtual async IAsyncEnumerable<Message> GetMessages(string contactId, DateTime? from)
        {
            List<Message> messages = null;
            var start = from ?? EARLIEST_API_TIME;

            var processed = new HashSet<string>();

            for (; ; )
            {
                messages = (await Cloud.GetMessages(start, 100, contactId, false).ConfigureAwait(false)).Entity;

                messages?.RemoveAll(m => processed.Contains(m.Id));

                if (messages == null || messages.Count == 0)
                    break;

                foreach (var m in messages)
                {
                    yield return m;

                    if (m.SendTime >= start)
                        start = m.SendTime;

                    processed.Add(m.Id);
                }
            }
        }

        public virtual async IAsyncEnumerable<GroupData> GetGroups()
        {
            var memberships = await Cloud.GetUserGroupMemeberships().ConfigureAwait(false);

            foreach (var membership in memberships.Entity)
            {
                var group = await Cloud.GetGroup(membership.GroupId).ConfigureAwait(false);

                var storage = await Cloud.GetStorage(membership.GroupId).ConfigureAwait(false);

                yield return new GroupData(group.Entity, storage.Entity);
            }
        }

        public virtual async Task<List<MemberData>> GetMembers(string groupId)
        {
            var members = await Cloud.GetGroupMembers(groupId).ConfigureAwait(false);

            var data = new List<MemberData>();

            foreach (var member in members.Entity)
            {
                var storage = await Cloud.GetMemberStorage(groupId, member.UserId).ConfigureAwait(false);

                data.Add(new MemberData(member, storage.Entity));
            }

            return data;
        }

        public virtual async Task<Record> GetRecord(string ownerId, string recordId)
        {
            var result = await Cloud.GetRecord<Record>(ownerId, recordId).ConfigureAwait(false);

            return result.Entity;
        }

        public virtual async IAsyncEnumerable<IEnumerable<Record>> GetRecords(string ownerId, DateTime? from)
        {
            var searchParams = new AccountDownloaderSearchParameters
            {
                ByOwner = ownerId,
                Private = true,

                SortBy = SearchSortParameter.LastUpdateDate,
                SortDirection = SearchSortDirection.Descending
            };

            if (from != null)
                searchParams.MinDate = from.Value;

            var search = new PaginatedRecordSearch<Record>(searchParams, Cloud, this.Logger);

            var count = 0;

            while (search.HasMoreResults)
            {
                count += 100;
                var records = await search.Next();
                yield return records;
            }
        }

        public virtual User GetUserMetadata() => Cloud.CurrentUser;

        public virtual async Task<List<CloudVariableDefinition>> GetVariableDefinitions(string ownerId)
        {
            var definitions = await Cloud.ListVariableDefinitions(ownerId).ConfigureAwait(false);
            return definitions.Entity;
        }

        public virtual async Task<List<CloudVariable>> GetVariables(string ownerId)
        {
            var variables = await Cloud.GetAllVariables(ownerId).ConfigureAwait(false);
            return variables.Entity;
        }

        public virtual async Task<CloudVariable> GetVariable(string ownerId, string path)
        {
            var result = await Cloud.ReadVariable<CloudVariable>(ownerId, path).ConfigureAwait(false);

            return result?.Entity;
        }

        public virtual async Task<DateTime> GetLatestMessageTime(string contactId)
        {
            int delay = 50;

            CloudResult lastResult = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                var messages = await Cloud.GetMessages(null, 1, contactId, false).ConfigureAwait(false);

                lastResult = messages;

                if (!messages.IsOK)
                {
                    await Task.Delay(delay);
                    delay *= 2;

                    continue;
                }

                if (messages.Entity.Count > 0)
                    return messages.Entity[0].LastUpdateTime;

                return EARLIEST_API_TIME;
            }

            throw new Exception($"Failed to determine latest message time after several attempts for contactId: {contactId}. Result: {lastResult}");
        }

        public virtual async Task<DateTime?> GetLatestRecordTime(string ownerId)
        {
            var search = new SearchParameters
            {
                ByOwner = ownerId,
                Private = true,
                SortBy = SearchSortParameter.LastUpdateDate,
                SortDirection = SearchSortDirection.Descending,
                Count = 1
            };

            var records = await Cloud.FindRecords<Record>(search).ConfigureAwait(false);

            if (records.IsOK && records.Entity.Records.Count == 1)
                return records.Entity.Records[0].LastModificationTime;

            return null;
        }

        public virtual async Task<long> GetAssetSize(string hash)
        {
            var result = await Cloud.GetGlobalAssetInfo(hash).ConfigureAwait(false);

            if (result.IsOK)
                return result.Entity.Bytes;
            else
                return 0;
        }

        public virtual Task<Stream> ReadAsset(string hash)
        {
            return WebClient.GetStreamAsync(CloudXInterface.NeosDBToHttp(new Uri($"{DB_PREFIX}{hash}"), NeosDB_Endpoint.Default));
        }

        public Task Cancel()
        {
            return Task.CompletedTask;
        }

//TODO: Enabling Nullable here temporarily. Full Nullables
#nullable enable
        public async Task<IExtensionResult?> GetAssetExtension(string hash)
        {
            var result = await Cloud.GetAssetMime(hash);
            if (result.IsOK)
            {
                var ext = MimeDetector.Instance.ExtensionFromMime(result.Content);

                return new ExtensionResult()
                {
                    MimeType = result.Content,
                    Extension = ext
                };
            }
            return null;
        }
    }
#nullable disable
}

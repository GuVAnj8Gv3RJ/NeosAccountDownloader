using System.Net;
using CloudX.Shared;

namespace AccountDownloaderLibrary
{
    public class CloudAccountDataStore : IAccountDataGatherer
    {
        public readonly CloudXInterface Cloud;

        public string Name => Cloud.UserAgentProduct + " " + Cloud.UserAgentVersion;
        public string UserId => Cloud.CurrentUser.Id;
        public string Username => Cloud.CurrentUser.Username;

#pragma warning disable CS0067 // The event 'CloudAccountDataStore.ProgressMessage' is never used
        public event Action<string> ProgressMessage;
#pragma warning restore CS0067 // The event 'CloudAccountDataStore.ProgressMessage' is never used

        public int FetchedGroupCount { get; private set; }

        readonly Dictionary<string, int> _fetchedRecords = new();

        public static DateTime EARLIEST_API_TIME = new(2016, 1, 1);

        private CancellationToken CancelToken;

        private const string DB_PREFIX = "neosdb:///";
        private HttpClient WebClient = new HttpClient();

        public int FetchedRecordCount(string ownerId)
        {
            _fetchedRecords.TryGetValue(ownerId, out var count);
            return count;
        }

        public CloudAccountDataStore(CloudXInterface cloud)
        {
            this.Cloud = cloud;
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

        public virtual async IAsyncEnumerable<Record> GetRecords(string ownerId, DateTime? from)
        {
            var searchParams = new SearchParameters
            {
                ByOwner = ownerId,
                Private = true,

                SortBy = SearchSortParameter.LastUpdateDate,
                SortDirection = SearchSortDirection.Descending
            };

            if (from != null)
                searchParams.MinDate = from.Value;

            var search = new RecordSearch<Record>(searchParams, Cloud);

            var count = 0;

            while (search.HasMoreResults)
            {
                count += 100;
                await search.EnsureResults(count);
            }

            _fetchedRecords[ownerId] = search.Records.Count;

            foreach (var r in search.Records)
            {
                string lastError = null;

                // Must get the actual record, which will include manifest
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    var result = await Cloud.GetRecord<Record>(ownerId, r.RecordId).ConfigureAwait(false);

                    if (result.Entity == null)
                    {
                        // it was deleted in the meanwhile, just ignore
                        if (result.State == HttpStatusCode.NotFound)
                            break;

                        lastError = $"Could not get record: {ownerId}:{r.RecordId}. Result: {result}";

                        // try again
                        continue;
                    }

                    yield return result.Entity;
                    break;
                }

                if (lastError != null)
                    throw new Exception(lastError);
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

        public virtual async Task DownloadAsset(string hash, string targetPath)
        {            
            await WebClient.DownloadFileTaskAsync(
                CloudXInterface.NeosDBToHttp(new Uri($"{DB_PREFIX}{hash}"), NeosDB_Endpoint.Default), targetPath);
        }

        public virtual async Task<long> GetAssetSize(string hash)
        {
            var result = await Cloud.GetGlobalAssetInfo(hash).ConfigureAwait(false);

            if (result.IsOK)
                return result.Entity.Bytes;
            else
                return 0;
        }

        public virtual async Task<string> GetAsset(string hash)
        {
            var tempPath = System.IO.Path.GetTempFileName();
            await DownloadAsset(hash, tempPath).ConfigureAwait(false);
            return tempPath;
        }

        public virtual Task<AssetData> ReadAsset(string hash)
        {
            return Task.FromResult<AssetData>(CloudXInterface.NeosDBToHttp(new Uri($"{DB_PREFIX}{hash}"), NeosDB_Endpoint.Default));
        }

        public Task Cancel()
        {
            return Task.CompletedTask;
        }
    }
}

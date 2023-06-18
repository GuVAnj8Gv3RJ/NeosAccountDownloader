using System.Threading.Tasks.Dataflow;
using System.Text.Json;
using CloudX.Shared;
using AccountDownloaderLibrary.Extensions;
using Medallion.Threading.FileSystem;
using ConcurrentCollections;

namespace AccountDownloaderLibrary
{
    public class LocalAccountDataStore : IAccountDataStore, IDisposable
    {
        readonly struct AssetJob
        {
            public readonly string hash;
            public readonly IAccountDataGatherer source;
            public readonly RecordStatusCallbacks callbacks;

            public AssetJob(string hash, IAccountDataGatherer source, RecordStatusCallbacks re)
            {
                this.hash = hash;
                this.source = source;
                this.callbacks = re;
            }
        }

        ActionBlock<AssetJob> DownloadProcessor;
        readonly ConcurrentHashSet<string> ScheduledAssets = new();

        public string Name => "Local Data Store";
        public string UserId { get; private set; }
        public string Username { get; private set; }

        public readonly string BasePath;
        public readonly string AssetsPath;
        private readonly AccountDownloadConfig Config;

        public event Action<string> ProgressMessage;

        private FileDistributedLockHandle DirectoryLock;

        public int FetchedGroupCount { get; private set; }

        readonly Dictionary<string, int> _fetchedRecords = new();

        private CancellationToken CancelToken;

        public int FetchedRecordCount(string ownerId)
        {
            _fetchedRecords.TryGetValue(ownerId, out var count);
            return count;
        }

        public LocalAccountDataStore(string userId, string basePath, string assetsPath, AccountDownloadConfig config)
        {
            UserId = userId;
            BasePath = basePath;
            AssetsPath = assetsPath;
            Config = config;
        }

        public async Task Prepare(CancellationToken token) {
            var lockFileDirectory = new DirectoryInfo(BasePath);
            CancelToken = token;

            InitDownloadProcessor(CancelToken);

            var myLock = new FileDistributedLock(lockFileDirectory, "AccountDownloader");
            try
            {
                DirectoryLock = await myLock.AcquireAsync(TimeSpan.FromSeconds(5), token);

                if (DirectoryLock != null)
                    return;
            }
            catch 
            {
                throw new DataStoreInUseException("Could not aquire a lock on LocalAccountStore is this path in use by another tool?");
            }
        }

        public async Task Complete()
        {
            DownloadProcessor.Complete();
            await DownloadProcessor.Completion.ConfigureAwait(false);

            ReleaseLocks();
        }

        void InitDownloadProcessor(CancellationToken token)
        {
            Directory.CreateDirectory(AssetsPath);

            DownloadProcessor = new ActionBlock<AssetJob>(async job =>
            {
                var path = GetAssetPath(job.hash);

                if (File.Exists(path))
                    return;

                try
                {
                    ProgressMessage?.Invoke($"Downloading asset {job.hash}");

                    await job.source.DownloadAsset(job.hash, path).ConfigureAwait(false);

                    job.callbacks.AssetUploaded();

                    ProgressMessage?.Invoke($"Finished download {job.hash}");
                }
                catch (Exception ex)
                {
                    ProgressMessage?.Invoke($"Exception {job.hash}: " + ex);
                }
            }, new ExecutionDataflowBlockOptions()
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = Config.MaxDegreeOfParallelism,
            });
        }

        public User GetUserMetadata() => GetEntity<User>(UserMetadataPath(UserId));

        public Task<List<Friend>> GetContacts() => GetEntities<Friend>(ContactsPath(UserId));

        public async IAsyncEnumerable<Message> GetMessages(string contactId, DateTime? from)
        {
            var messages = await GetEntities<Message>(MessagesPath(UserId, contactId)).ConfigureAwait(false);

            foreach (var msg in messages)
            {
                if (from != null && msg.LastUpdateTime < from.Value)
                    continue;

                yield return msg;
            }
        }

        public async Task<Record> GetRecord(string ownerId, string recordId)
        {
            return await Task.FromResult(GetEntity<Record>(Path.Combine(RecordsPath(ownerId), recordId)));
        }

        public async IAsyncEnumerable<Record> GetRecords(string ownerId, DateTime? from)
        {
            var records = await GetEntities<Record>(RecordsPath(ownerId)).ConfigureAwait(false);

            _fetchedRecords[ownerId] = records.Count;

            foreach (var record in records)
            {
                if (from != null && record.LastModificationTime < from.Value)
                    continue;

                yield return record;
            }
        }

        public Task<List<CloudVariableDefinition>> GetVariableDefinitions(string ownerId) => GetEntities<CloudVariableDefinition>(VariableDefinitionPath(ownerId));

        public Task<List<CloudVariable>> GetVariables(string ownerId) => GetEntities<CloudVariable>(VariablePath(ownerId));

        public async Task<CloudVariable> GetVariable(string ownerId, string path)
        {
            return await Task.FromResult(GetEntity<CloudVariable>(Path.Combine(VariablePath(ownerId), path)));
        }

        public async IAsyncEnumerable<GroupData> GetGroups()
        {
            var path = GroupsPath(UserId);
            var groups = await GetEntities<Group>(path).ConfigureAwait(false);

            FetchedGroupCount = groups.Count;

            foreach (var group in groups)
            {
                var storage = GetEntity<Storage>(Path.Combine(path, group.GroupId + ".Storage"));

                yield return new GroupData(group, storage);
            }
        }

        public async Task<List<MemberData>> GetMembers(string groupId)
        {
            var path = MembersPath(UserId, groupId);
            var members = await GetEntities<Member>(path).ConfigureAwait(false);

            var list = new List<MemberData>();

            foreach (var member in members)
            {
                var storage = GetEntity<Storage>(Path.Combine(path, member.UserId + ".Storage"));

                list.Add(new MemberData(member, storage));
            }

            return list;
        }

        static Task<List<T>> GetEntities<T>(string path)
        {
            var list = new List<T>();

            if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.json"))
                {
                    var entity = JsonSerializer.Deserialize<T>(File.ReadAllText(file));

                    list.Add(entity);
                }
            }

            return Task.FromResult(list);
        }

        static T GetEntity<T>(string path)
        {
            path += ".json";

            if (File.Exists(path))
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path));

            return default;
        }

        public async Task StoreDefinitions(List<CloudVariableDefinition> definitions)
        {
            foreach (var definition in definitions)
                await StoreEntity(definition, Path.Combine(VariableDefinitionPath(definition.DefinitionOwnerId), definition.Subpath)).ConfigureAwait(false);
        }

        public async Task StoreVariables(List<CloudVariable> variables)
        {
            foreach (var variable in variables)
                await StoreEntity(variable, Path.Combine(VariablePath(variable.VariableOwnerId), variable.Path)).ConfigureAwait(false);
        }

        public Task StoreUserMetadata(User user) => StoreEntity(user, Path.Combine(UserMetadataPath(user.Id)));

        public Task StoreContact(Friend cotnact) => StoreEntity(cotnact, Path.Combine(ContactsPath(cotnact.OwnerId), cotnact.FriendUserId));

        public Task StoreMessage(Message message) => StoreEntity(message, Path.Combine(MessagesPath(message.OwnerId, message.GetOtherUserId()), message.Id));

        public async Task<string> StoreRecord(Record record, IAccountDataGatherer source, RecordStatusCallbacks statusCallbacks, bool overwriteOnConflict)
        {
            await StoreEntity(record, Path.Combine(RecordsPath(record.OwnerId), record.RecordId)).ConfigureAwait(false);

            if (record.NeosDBManifest != null)
                foreach (var asset in record.NeosDBManifest)
                    ScheduleAsset(asset.Hash, source, statusCallbacks);

            return null;
        }

        public async Task StoreGroup(Group group, Storage storage)
        {
            var path = Path.Combine(GroupsPath(group.AdminUserId), group.GroupId);

            await StoreEntity(group, path);
            await StoreEntity(storage, path + ".Storage");
        }

        public async Task StoreMember(Group group, Member member, Storage storage)
        {
            var path = Path.Combine(MembersPath(group.AdminUserId, member.GroupId), member.UserId);

            await StoreEntity(member, path);
            await StoreEntity(storage, path + ".Storage");
        }

        static Task StoreEntity<T>(T entity, string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(entity);

            File.WriteAllText(path + ".json", json);

            return Task.CompletedTask;
        }

        string VariableDefinitionPath(string ownerId) => Path.Combine(BasePath, ownerId, "VariableDefinitions");
        string VariablePath(string ownerId) => Path.Combine(BasePath, ownerId, "Variables");
        string UserMetadataPath(string ownerId) => Path.Combine(BasePath, ownerId, "User");
        string ContactsPath(string ownerId) => Path.Combine(BasePath, ownerId, "Contacts");
        string MessagesPath(string ownerId, string contactId) => Path.Combine(BasePath, ownerId, "Messages", contactId);
        string RecordsPath(string ownerId) => Path.Combine(BasePath, ownerId, "Records");
        string GroupsPath(string ownerId) => Path.Combine(BasePath, ownerId, "Groups");
        string MembersPath(string ownerId, string groupId) => Path.Combine(BasePath, ownerId, "GroupMembers", groupId);
        string GetAssetPath(string hash) => Path.Combine(AssetsPath, hash);

        public async Task<DateTime> GetLatestMessageTime(string contactId)
        {
            DateTime latest = new(2016, 1, 1);

            await foreach (var message in GetMessages(contactId, null).ConfigureAwait(false))
            {
                if (message.LastUpdateTime > latest)
                    latest = message.LastUpdateTime;
            }

            return latest;
        }

        public async Task<DateTime?> GetLatestRecordTime(string ownerId)
        {
            DateTime? latest = null;

            await foreach (var record in GetRecords(ownerId, null).ConfigureAwait(false))
            {
                if (latest == null || record.LastModificationTime > latest)
                    latest = record.LastModificationTime;
            }

            return latest;
        }

        void ScheduleAsset(string hash, IAccountDataGatherer store, RecordStatusCallbacks recordStatusCallbacks)
        {
            if (!ScheduledAssets.Add(hash))
                return;

            var job = new AssetJob(hash, store, recordStatusCallbacks);

            // TODO: I forget where we were meant to get this info from.
            var diff = new AssetDiff();
            diff.Bytes = 0;

            recordStatusCallbacks.AssetToUploadAdded(diff);

            DownloadProcessor.Post(job);
        }

        public Task DownloadAsset(string hash, string targetPath)
        {
            return Task.Run(() => File.Copy(GetAssetPath(hash), targetPath));
        }

        public Task<long> GetAssetSize(string hash)
        {
            var path = GetAssetPath(hash);

            if (File.Exists(path))
                return Task.FromResult(new FileInfo(GetAssetPath(hash)).Length);
            else
                return Task.FromResult(0L);
        }

        public Task<string> GetAsset(string hash)
        {
            return Task.FromResult(GetAssetPath(hash));
        }

        public Task<AssetData> ReadAsset(string hash) => Task.FromResult<AssetData>(File.OpenRead(GetAssetPath(hash)));

        private void ReleaseLocks()
        {
            DirectoryLock?.Dispose();
        }
        public void Dispose()
        {
            ReleaseLocks();
        }

        public Task Cancel()
        {
            ReleaseLocks();
            DownloadProcessor.Complete();
            return Task.CompletedTask;
        }
    }
}

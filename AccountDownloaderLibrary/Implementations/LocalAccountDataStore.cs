using AccountDownloaderLibrary.Extensions;
using AccountDownloaderLibrary.Mime;
using BaseX;
using CloudX.Shared;
using ConcurrentCollections;
using Medallion.Threading.FileSystem;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AccountDownloaderLibrary
{
    public class LocalAccountDataStore : IAccountDataStore, IDisposable
    {
        readonly struct AssetJob
        {
            public readonly NeosDBAsset asset;
            public readonly Record forRecord;
            public readonly IAccountDataGatherer source;
            public readonly RecordStatusCallbacks callbacks;

            public AssetJob(Record forRecord, NeosDBAsset asset, IAccountDataGatherer source, RecordStatusCallbacks re)
            {
                this.asset = asset;
                this.source = source;
                this.callbacks = re;
                this.forRecord = forRecord;
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

        private readonly ILogger Logger;

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

        public LocalAccountDataStore(string userId, string basePath, string assetsPath, AccountDownloadConfig config, ILogger logger)
        {
            UserId = userId;
            BasePath = basePath;
            AssetsPath = assetsPath;
            Config = config;

            Logger = logger;
        }

        public async Task Prepare(CancellationToken token)
        {
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
                Logger.LogError("Could not aquire a lock on LocalAccountStore is this path in use by another tool?");
                throw new DataStoreInUseException("Could not aquire a lock on LocalAccountStore is this path in use by another tool?");
            }
        }

        public async Task Complete()
        {
            DownloadProcessor.Complete();
            await DownloadProcessor.Completion.ConfigureAwait(false);

            ReleaseLocks();
        }

        // These are mimes which Neos is potentially confused or weirded out by
        // Was going to call these "SussyMimes"

        // Move to config maybe?
        private readonly HashSet<string> AmbiguousMimes = new()
        {
            "application/octet-stream",
        };

        private void PostProcessAmbiguousMime(string path, IExtensionResult res, string hash)
        {
            if (!AmbiguousMimes.Contains(res.MimeType))
                return;

            var detectedExtension = MimeDetector.Instance.MostLikelyFileExtension(path);
            if (detectedExtension == null)
                return;

            var existingExtension = Path.GetExtension(path).Replace(".", "");

            if (existingExtension != null && existingExtension == detectedExtension)
                return;

            Logger.LogInformation("Mime Analysis for Asset: {hash} discovered a new extension, {detectedExtension}", hash, detectedExtension);

            var newPath = $"{Path.GetFileNameWithoutExtension(path)}.{detectedExtension}";

            File.Move(path, newPath, true);
        }

        // Our extension code is always improving so this method will continually do additionall processing to discover new extension results/update ones.
        private void PostProcessAsset(string storedPath, string hash, IExtensionResult extResult)
        {
            // However, post-process anything ambiguous, we could also do additional processing here
            PostProcessAmbiguousMime(storedPath, extResult, hash);

        }
#nullable enable
        //TODO: Retries
        private async Task ProcessJob(AssetJob job)
        {
            // Somehow the DownloadProcessor was processing jobs after completion, This lead to some really weird outputs such as files being written to the
            // App's working directory.
            // See: https://github.com/GuVAnj8Gv3RJ/NeosAccountDownloader/issues/202
            // Do not process if the downloader has completed, should fix 202.
            if (DownloadProcessor.Completion.IsCompleted)
                return;

            string originalPath = GetAssetPath(job.asset.Hash);
            IExtensionResult? extResult = await job.source.GetAssetExtension(job.asset.Hash);

            string extensionPath = extResult?.Extension != null ? $"{originalPath}.{extResult.Extension}" : originalPath;

            // When it comes to Mimetypes, we start with the principle of "Trust what neos says", so ask it what the extension should be

            if (extResult?.Extension == null)
            {
                Logger.LogInformation("Asset: {hash} with: {mime} has a missing extension", job.asset.Hash, extResult?.MimeType);
            }

            try
            {
                // File exists at original path and we have a new path for it. Move
                // This is mostly for users running new versions of the downloader, before we added extensions
                if (File.Exists(originalPath) && extensionPath != originalPath)
                {
                    Logger.LogInformation("Discovered extension for Asset: {hash}, {extension}", job.asset.Hash, extResult?.Extension);

                    // Mark this file as skiped, we don't need to re-download it.
                    job.callbacks.AssetSkipped?.Invoke(job.asset.Hash);

                    File.Move(originalPath, extensionPath, true);

                    PostProcessAsset(extensionPath, job.asset.Hash, extResult);
                    return;
                }

                // File is in the correct location with Extension
                if (File.Exists(extensionPath))
                {
                    Logger.LogInformation("Asset: {hash}, was already downloaded skipping", job.asset.Hash);
                    // Mark this file as skiped, we don't need to re-download it.
                    job.callbacks.AssetSkipped?.Invoke(job.asset.Hash);

                    PostProcessAsset(extensionPath, job.asset.Hash, extResult);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in processing asset with Hash: {hash}: {message}", job.asset.Hash, ex.Message);
                job.callbacks.AssetFailure?.Invoke(new AssetFailure(job.asset.Hash, ex.Message, job.forRecord));
            }

            // Past here we haven't downloaded the asset at all, download it fresh
            try
            {
                ProgressMessage?.Invoke($"Downloading asset {job.asset.Hash}");

                using (Stream data = await job.source.ReadAsset(job.asset.Hash).ConfigureAwait(false))
                {
                    using FileStream fs = new(extensionPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    await data.CopyToAsync(fs);
                }

                // We have to perform sussy mime checks once the asset is fully downloaded
                PostProcessAsset(extensionPath, job.asset.Hash, extResult);

                job.callbacks.AssetUploaded?.Invoke(job.asset.Hash);

                ProgressMessage?.Invoke($"Finished download {job.asset.Hash}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in fetching asset with Hash: {hash}: {ex}", job.asset.Hash, ex.Message);
                job.callbacks.AssetFailure?.Invoke(new AssetFailure(job.asset.Hash, ex.Message, job.forRecord));
            }
        }
#nullable disable
        void InitDownloadProcessor(CancellationToken token)
        {
            Directory.CreateDirectory(AssetsPath);

            // Using a non-anonymous function causes better stack traces
            DownloadProcessor = new ActionBlock<AssetJob>(ProcessJob, new ExecutionDataflowBlockOptions()
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

        public async IAsyncEnumerable<IEnumerable<Record>> GetRecords(string ownerId, DateTime? from)
        {
            var records = await GetEntities<Record>(RecordsPath(ownerId)).ConfigureAwait(false);

            //TODO: honor "From"
            yield return records;
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
                    ScheduleAsset(record, asset, source, statusCallbacks);

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
            // Don't write nulls to the file system
            if (entity == null)
                return Task.CompletedTask;

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

            await foreach (var page in GetRecords(ownerId, null).ConfigureAwait(false))
            {
                foreach (var record in page)
                {
                    if (latest == null || record.LastModificationTime > latest)
                        latest = record.LastModificationTime;
                }
            }

            return latest;
        }

        void ScheduleAsset(Record record, NeosDBAsset asset, IAccountDataGatherer store, RecordStatusCallbacks recordStatusCallbacks)
        {
            if (!ScheduledAssets.Add(asset.Hash))
                return;

            var job = new AssetJob(record, asset, store, recordStatusCallbacks);

            var diff = new AssetDiff
            {
                State = AssetDiff.Diff.Added,
                Bytes = asset.Bytes
            };

            recordStatusCallbacks.AssetToUploadAdded(diff);

            DownloadProcessor.Post(job);
        }

        public Task<long> GetAssetSize(string hash)
        {
            var path = GetAssetPath(hash);

            if (File.Exists(path))
                return Task.FromResult(new FileInfo(GetAssetPath(hash)).Length);
            else
                return Task.FromResult(0L);
        }

        public Task<Stream> ReadAsset(string hash)
        {
            return Task.FromResult<Stream>(File.OpenRead(GetAssetPath(hash)));
        }

        private void ReleaseLocks()
        {
            DirectoryLock?.Dispose();
        }
        public void Dispose()
        {
            ReleaseLocks();
            GC.SuppressFinalize(this);
        }

        public Task Cancel()
        {
            ReleaseLocks();
            DownloadProcessor.Complete();
            return Task.CompletedTask;
        }

        Task<IExtensionResult> IAccountDataGatherer.GetAssetExtension(string hash)
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}

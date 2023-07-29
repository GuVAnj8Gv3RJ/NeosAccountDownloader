using CloudX.Shared;

namespace AccountDownloaderLibrary
{
    public readonly struct GroupData
    {
        public readonly Group group;
        public readonly Storage storage;

        public GroupData(Group group, Storage storage)
        {
            this.group = group;
            this.storage = storage;
        }
    }

    public readonly struct MemberData
    {
        public readonly Member member;
        public readonly Storage storage;

        public MemberData(Member member, Storage storage)
        {
            this.member = member;
            this.storage = storage;
        }
    }

    public class AssetFailure
    {
        public string Hash { get; }
        public string Reason { get; }
        public Record ForRecord { get; }

        public string RecordName => ForRecord?.Name ?? "Unknown";
        public string OwnerId => ForRecord?.OwnerId ?? "Unknown";
        public string RecordPath => (ForRecord?.Path != string.Empty ? ForRecord?.Path : "Root/World") ?? "Unknown";

        public AssetFailure(string hash, string reason, Record forRecord)
        {
            this.Hash = hash;
            this.Reason = reason;
            this.ForRecord = forRecord;
        }
    }

    public enum AssetResultType
    {
        Unknown = 0,
        Downloaded,
        Skipped,
        Error
    }
    public class AssetResult
    {
        public string Hash { get; }
        public string Reason { get; }
        public AssetResultType ResultType { get; }
    }

    public class RecordStatusCallbacks
    {
        public Action<AssetDiff> AssetToUploadAdded;
        public Action<long> BytesUploaded;
        public Action<string> AssetUploaded;
        public Action<AssetFailure> AssetFailure;
        public Action<string> AssetSkipped;
    }

    public interface IAccountDataGatherer
    {
        public string Name { get; }
        public string UserId { get; }
        public string Username { get; }

        public event Action<string> ProgressMessage;

        int FetchedRecordCount(string ownerId);
        int FetchedGroupCount { get; }
        Task Prepare(CancellationToken token);
        Task Complete();

        // Prepare and Complete represent Start and Finish and need to do certain cleanup.
        // Here we expose Cancel as well which lets Stores handle anything they need to do when a cancellation occurs
        Task Cancel();

        Task<long> GetAssetSize(string hash);
        Task DownloadAsset(string hash, string targetPath);
        Task<string> GetAsset(string hash);
        Task<AssetData> ReadAsset(string hash);
        Task<string> GetAssetExtension(string hash);

        Task<List<CloudVariableDefinition>> GetVariableDefinitions(string ownerId);
        Task<CloudVariable> GetVariable(string ownerId, string path);
        Task<List<CloudVariable>> GetVariables(string ownerId);
        IAsyncEnumerable<GroupData> GetGroups();
        Task<List<MemberData>> GetMembers(string groupId);
        Task<Record> GetRecord(string ownerId, string recordId);
        IAsyncEnumerable<Record> GetRecords(string ownerId, DateTime? from);
        User GetUserMetadata();
        Task<List<Friend>> GetContacts();
        IAsyncEnumerable<Message> GetMessages(string contactId, DateTime? from);
        Task<DateTime?> GetLatestRecordTime(string ownerId);
        Task<DateTime> GetLatestMessageTime(string contactId);
    }

    public interface IAccountDataStore: IAccountDataGatherer
    {
        Task StoreDefinitions(List<CloudVariableDefinition> definition);
        Task StoreVariables(List<CloudVariable> variable);
        Task StoreGroup(Group group, Storage storage);
        Task StoreMember(Group group, Member member, Storage storage);
        Task<string> StoreRecord(Record record, IAccountDataGatherer source, RecordStatusCallbacks statusCallbacks = null, bool forceConflictOverwrite = false);
        Task StoreUserMetadata(User user);
        Task StoreContact(Friend friend);
        Task StoreMessage(Message message);
    }
}

using AccountDownloaderLibrary;
using AccountDownloaderLibrary.Interfaces;
using CloudX.Shared;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AccountDownloader.Services
{
    public interface IStorageRecord : IReactiveNotifyPropertyChanged<IReactiveObject>
    {
        public long UsedBytes { get; set; }
        public long TotalBytes { get; set; }

        public string FormattedUsed { get; }
        public string FormattedTotal { get; }

        public void Update(long used, long total);

        public string Id { get; set; }
        public OwnerType OwnerType { get; set; }

    }
    public interface IStorageService
    {
        public IStorageRecord GetUserStorage();
        public IStorageRecord GetGroupStorage(string groupId);
    }
    public readonly struct AuthResult
    {
        public readonly AuthenticationState state;
        public readonly string? error;

        public AuthResult(AuthenticationState state, string? error)
        {
            this.state = state;
            this.error = error;
        }
    }
    public enum AuthenticationState
    {
        Unauthenticated = 0,
        Authenticating,
        Authenticated,
        TOTPRequired,
        Error
    }

    public interface IAppCloudService
    {
        public AuthenticationState AuthState { get; }
        public Task<AuthResult> Login(string username, string password);
        public Task<AuthResult> Logout();
        public Task<AuthResult> SubmitTOTP(string TOTP);

        public IUserProfile Profile { get; }
    }

    public interface IAccountDownloader
    {
        public Action<string>? ProgressMessageHandler { get; set; }
        public string LatestProgressMessage { get; }

        public string? DownloadPhase { get; }
        public Task<IDownloadResult> Start(IAccountDownloadConfig config);

        public AccountDownloadStatus? Status { get; }
        public void Cancel();
    }

    public interface IAccountDownloadConfig: IReactiveNotifyPropertyChanged<IReactiveObject>
    {
        public bool UserMetadata { get; }
        public bool Contacts { get; }
        public bool MessageHistory { get; }
        public bool InventoryWorlds { get; }
        public bool CloudVariableDefinitions { get; }
        public bool CloudVariableValues { get; }

        public bool OnlyNewRecords { get; }
        public DateTime? RecordsFrom { get; }

        public IEnumerable<string> Groups { get; }

        public string FilePath { get; }
    }

    public interface IUserProfile: IReactiveNotifyPropertyChanged<IReactiveObject>
    {
        public string UserName { get; }
        public Uri? PictureURI { get; }

        void UpdateUser(User obj);
    }

    public interface IGroup: INotifyPropertyChanged
    {
        public string Name { get; }
        public string Id { get; }
        public bool IsAdmin { get; }

        public IStorageRecord Storage { get; }

        public bool ShouldDownload { get; set; }
        public long RequiredBytes { get; }
    }
    public interface IGroupsService
    {
        public Task<List<IGroup>> GetGroups();
    }
}

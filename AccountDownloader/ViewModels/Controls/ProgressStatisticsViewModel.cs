using AccountDownloader.Services;
using AccountDownloaderLibrary;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AccountDownloader.ViewModels
{
	public class ProgressStatisticsViewModel : ReactiveObject
	{
        [Reactive]
        public IAccountDownloadConfig Config { get; private set; }
        [Reactive]
        public AccountDownloadStatus Status { get; private set; }

        public ProgressStatisticsViewModel(IAccountDownloadConfig config, AccountDownloadStatus status)
        {
            Config = config;
            Status = status;
        }
    }
}

using AccountDownloader.Services;
using AccountDownloaderLibrary;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Reactive;

namespace AccountDownloader.ViewModels
{
    // This is just to give us another page to look at.
	public class CompleteViewModel : ViewModelBase
    {
        public AccountDownloadStatus Status { get; }
        public IAccountDownloadConfig Config { get; }

        public ProgressStatisticsViewModel ProgressStatistics { get; }

        public FailedRecordsViewModel FailedRecords { get; }

        [Reactive]
        public bool ShouldShowFailureMessage { get; set; } = false;

        public ReactiveCommand<Unit, IRoutableViewModel> StartAnotherDownload { get; }
        public ReactiveCommand<Unit, Unit> Exit { get; }
        public CompleteViewModel(IAccountDownloadConfig config, AccountDownloadStatus status)
        {
            Status = status;
            Config = config;
            ProgressStatistics = new ProgressStatisticsViewModel(config, status);

            // Zip up all the records that have failed
            List<RecordDownloadFailure> list = new(status.UserRecordsStatus.FailedRecords);
            foreach (var g in Status.GroupStatuses)
            {
                list.AddRange(g.RecordsStatus.FailedRecords);
            }

            FailedRecords = new FailedRecordsViewModel(list, Status.AssetFailures);

            if (list.Count > 0 || Status.AssetFailures.Count > 0)
                ShouldShowFailureMessage = true;

            StartAnotherDownload = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new DownloadSelectionViewModel()));
            Exit = ReactiveCommand.Create(ExitFn);

        }

        private void ExitFn()
        {
            //TODO: Test on more exotic os
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}

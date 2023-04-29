using AccountDownloader.Services;
using AccountDownloader.Utilities;
using AccountDownloader.Views;
using AccountDownloaderLibrary;
using Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace AccountDownloader.ViewModels;

public class ProgressViewModel : ViewModelBase
{
    public enum DownloadCancelResult
    {
        CancelationNotPossible = 0,
        CancellationCancelled,
        Cancelled,
    }

    [Reactive]
    public string ProgressText { get; set; } = string.Empty;

    [Reactive]
    public string? ProgressPhase { get; set; } = string.Empty;

    [Reactive]
    public bool ShouldShowPhase { get; set; } = false;
    public AccountDownloadStatus? Status => Downloader.Status;

    [Reactive]
    public IAccountDownloadConfig Config { get; private set; }

    [Reactive]
    public UserProfileViewModel ProfileViewModel { get; set; }

    public ReactiveCommand<Unit, DownloadCancelResult> Cancel { get; }

    [Reactive]
    public bool IsRunning { get; set; } = false;

    public ProgressStatisticsViewModel ProgressStatistics { get; }

    // Non-UI Stuff
    private IAppCloudService CloudService { get; }
    private IAccountDownloader Downloader { get; }

    public ProgressViewModel(IAccountDownloadConfig config)
    {
        CloudService = Locator.Current.GetService<IAppCloudService>() ?? throw new NullReferenceException("Cannot login without an app service");
        Downloader = Locator.Current.GetService<IAccountDownloader>() ?? throw new NullReferenceException("Cannot download an account without a downloader");

        ProfileViewModel = new UserProfileViewModel(CloudService.Profile);

        Downloader.ProgressMessageHandler += SetProgressText;


        this.WhenAnyValue(x => x.ProgressPhase).Subscribe(x => ShouldShowPhase = x != null);

        Cancel = ReactiveCommand.CreateFromTask(() => CancelDownload());
        Config = config;

        GlobalInteractions.OnMainWindowClose.RegisterHandler(async input =>
        {
            // If the user closes the window and we're running a download
            // Ask the user if they want to cancel the download
            var result = await CancelDownload();

            // If the result is CancellationCancelled, then
            // they do not want to cancel the download
            // In these cases, prevent the window from closing by setting output to "True"
            if (result == DownloadCancelResult.CancellationCancelled)
                input.SetOutput(true);
        });

        _ = StartDownload(config);

        ProgressStatistics = new ProgressStatisticsViewModel(config, Downloader.Status!);
    }

    private async Task StartDownload(IAccountDownloadConfig config)
    {
        IsRunning = true;
        var res = await Downloader.Start(config);
        await DownloadComplete(res);
    }

    private async Task DownloadComplete(IDownloadResult result)
    {
        IsRunning = false;
        switch(result.Result)
        {
            case DownloadResultType.Sucessful:
                await HandleSucces();
                break;
            case DownloadResultType.Cancelled:
                await HandleCancel();
                break;
            case DownloadResultType.Error:
            default:
                await HandleFailure(result);
                break;
        }
    }

    private async Task HandleCancel()
    {
        await GlobalInteractions.ShowMessageBox.Handle(new MessageBoxRequest(Res.Errors_DownloadCancelled));
        await Router.Navigate.Execute(new GettingStartedViewModel());
    }

    private async Task HandleFailure(IDownloadResult result)
    {
        // The error here might be a full stack trace.
        await GlobalInteractions.ShowMessageBox.Handle(new MessageBoxRequest(string.Format(Res.Errors_DownloadFailure, result.Error)));
        await Router.Navigate.Execute(new GettingStartedViewModel());
    }

    private async Task HandleSucces()
    {

        await GlobalInteractions.ShowMessageBox.Handle(new MessageBoxRequest(Res.DownloadComplete));

        await Router.Navigate.Execute(new CompleteViewModel(Config, Status!));
    }

    private void SetProgressText(string text)
    {
        ProgressPhase = Downloader.DownloadPhase;
        ProgressText = text;
    }

    private async Task<DownloadCancelResult> CancelDownload()
    {

        if (!IsRunning)
            return DownloadCancelResult.CancelationNotPossible;

        var result = await GlobalInteractions.ShowYesNoBox.Handle(new MessageBoxRequest(Res.AreYouSureDownloadCancel));

        if (result.Result == YesNo.No)
            return DownloadCancelResult.CancellationCancelled;

        Downloader.Cancel();
        IsRunning = false;

        return DownloadCancelResult.Cancelled;
    }
}

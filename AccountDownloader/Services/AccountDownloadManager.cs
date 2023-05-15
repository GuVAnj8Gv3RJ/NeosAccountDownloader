

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AccountDownloaderLibrary;
using AccountDownloaderLibrary.Interfaces;
using Avalonia.Threading;
using CloudX.Shared;
using Microsoft.Extensions.Logging;

namespace AccountDownloader.Services;
public class AccountDownloadManager : IAccountDownloader
{
    private CloudXInterface Interface { get;}

    private AccountDownloadController? Controller { get; set; }
    public Action<string>? ProgressMessageHandler { get; set; } = null;

    public string? DownloadPhase => Controller?.Status.Phase;

    public AccountDownloadStatus? Status => Controller?.Status;

    private CancellationTokenSource? CancelTokenSource = null;

    private readonly ILogger Logger;

    private DispatcherTimer StatsTimer = new DispatcherTimer();

    public AccountDownloadManager(CloudXInterface? cloudInterface, ILogger? logger)
    {
        Logger = logger ?? throw new NullReferenceException("Cannot run without a logger"); ;
        Interface = cloudInterface ?? throw new NullReferenceException("Cannot run without a CloudX Interface");
        StatsTimer.Interval = TimeSpan.FromSeconds(1);
        StatsTimer.Tick += StatsTimer_Tick;
    }

    private void StatsTimer_Tick(object? sender, EventArgs e)
    {
        if (Controller == null)
            return;
        Controller.Status?.UpdateStats();
    }

    // Here we're just hiding the implementation from consumers of AccountDownloader, it seems messy but Interfaces make this easier to develop
    // and easier to swap stuff in and out.
    private void SurfaceProgressMessage(string message)
    {
        Logger.LogInformation("Progress Message:{phase} - {message}", Controller?.Status.Phase ?? "Pending", message);

        ProgressMessageHandler?.Invoke(message);
    }

    public string LatestProgressMessage => Controller?.ProgressMessage ?? string.Empty;

    public void Cancel()
    {
        this.Logger.LogInformation("Cancelling download for {user}", Interface.CurrentUser.Username);
        if (CancelTokenSource != null && CancelTokenSource.Token.CanBeCanceled)
        {
            StatsTimer.Stop();
            CancelTokenSource.Cancel();
        }
    }

    public async Task<IDownloadResult> Start(IAccountDownloadConfig config)
    {
        this.Logger.LogInformation("Starting download for {user}", Interface.CurrentUser.Username);
        this.Logger.LogInformation("Saving output to {filePath}", config.FilePath);
        this.Logger.LogInformation("With config {config}", config);
        CancelTokenSource = new CancellationTokenSource();

        // We do not include the user's username here as CloudX takes care of this.
        // It'll store items owned by a user in a folder based on their User ID.
        var local = new LocalAccountDataStore(Interface.CurrentUser.Id, config.FilePath, config.FilePath + "/Assets");
        Controller = new AccountDownloadController(new CloudAccountDataStore(Interface), local, CreateConfigFromIAccountDownloadConfig(config));
        Controller.ProgressMessagePosted += SurfaceProgressMessage;
        StatsTimer.Start();

        var res = await StartDownload();

        // Log a status report
        if (Status != null)
            Logger.LogInformation(Status.GenerateReport());

        return res;
    }

    private async Task<IDownloadResult> StartDownload()
    {
        var result = await Controller!.Download(CancelTokenSource!.Token);
        this.Logger.LogInformation("Download Finished with result: {result}", result.Result.ToString());

        StatsTimer.Stop();

        if (result.Result == DownloadResultType.Error)
        {
            this.Logger.LogError(Controller?.Status?.Error);
            if (result.Exception != null)
                // Log the whole damn exception too!
                this.Logger.LogError(result.Exception.ToString());
        }

        return result;
    }

    private static AccountDownloadConfig CreateConfigFromIAccountDownloadConfig(IAccountDownloadConfig config)
    {
        var downloadConfig = new AccountDownloadConfig
        {
            DownloadCloudVariableDefinitions = config.CloudVariableDefinitions,
            DownloadCloudVariables = config.CloudVariableValues,
            DownloadUserMetadata = config.UserMetadata,
            DownloadContacts = config.Contacts,
            DownloadMessageHistory = config.MessageHistory,
            DownloadUserRecords = config.InventoryWorlds,

            // Prevent download of groups that haven't been selected
            DownloadGroups = config.Groups.Any(),
            GroupsToDownload = new HashSet<string>(config.Groups)
        };

        return downloadConfig;
    }
}

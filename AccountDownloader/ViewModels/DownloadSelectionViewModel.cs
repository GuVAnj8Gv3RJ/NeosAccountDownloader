using AccountDownloader.Services;
using AccountDownloader.Utilities;
using Avalonia.Platform.Storage;
using BaseX;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AccountDownloader.Extensions;

namespace AccountDownloader.ViewModels;

public class DownloadSelectionViewModel : ViewModelBase, IAccountDownloadConfig
{
    public ReactiveCommand<Unit, Unit> OpenFolder { get; }
    public Interaction<FolderPickerOpenOptions, InteractionResult<Uri>> ShowOpenFolderDialog { get; }
    private readonly IAppCloudService CloudService;
    private readonly IGroupsService GroupService;
    private readonly IStorageService StorageService;
    public UserProfileViewModel ProfileViewModel { get; }
    public GroupsListViewModel GroupsList { get; }

    public ReactiveCommand<Unit,Unit> StartDownload { get; }

    // IAccountDownloadConfig stuff
    // Is bound from the UI to select what data to download
    [Reactive]
    public bool UserMetadata { get; set; } = false;

    [Reactive]
    public bool Contacts { get; set; } = false;

    private bool PreviousMessageHistory = false;
    [Reactive]
    public bool MessageHistory { get; set; } = false;

    [Reactive]
    public bool InventoryWorlds { get; set; } = false;

    [Reactive]
    public bool CloudVariableDefinitions { get; set; } = false;

    [Reactive]
    public bool CloudVariableValues { get; set; } = false;

    [Reactive]
    public string FilePath { get; set; } = "";

    [ObservableAsProperty]
    public long RequiredBytes { get; } = 0;

    [ObservableAsProperty]
    public bool ShouldShowRequiredBytes { get; } = false;

    public IStorageRecord UserStorage { get; }

    public IEnumerable<string> Groups => GroupsList.GetSelectedGroupIds();

    public DownloadSelectionViewModel()
    {
        // Most of these will never trigger as our config is static, but Nullables make us do this and I kinda like how it'll point to a config issue.
        CloudService = Locator.Current.GetService<IAppCloudService>() ?? throw new NullReferenceException("Cannot download without an app service");
        GroupService = Locator.Current.GetService<IGroupsService>() ?? throw new NullReferenceException("Cannot download without a group service");
        StorageService = Locator.Current.GetService<IStorageService>() ?? throw new NullReferenceException("Cannot download without a storage service");

        // The OpenFile command is bound to a button/menu item in the UI.
        OpenFolder = ReactiveCommand.CreateFromTask(PickDownloadFolder);

        // The ShowOpenFileDialog interaction requests the UI to show the file open dialog.
        ShowOpenFolderDialog = new Interaction<FolderPickerOpenOptions, InteractionResult<Uri>>();

        ProfileViewModel = new UserProfileViewModel(CloudService.Profile);
        GroupsList = new GroupsListViewModel();
        UserStorage = StorageService.GetUserStorage();

        // This is kinda gross
        var hasDownloadSelection = this.WhenAnyValue(
            x => x.UserMetadata,
            x => x.Contacts,
            x => x.MessageHistory,
            x => x.InventoryWorlds,
            x => x.CloudVariableDefinitions,
            x => x.CloudVariableValues,
            (userMetadata, contacts, messagehistory, inventoryWorlds, cloudVariableDefinitions, cloudVariableValues) => userMetadata || contacts || messagehistory || inventoryWorlds || cloudVariableDefinitions || cloudVariableValues
        );
        var hasFilePath = this.WhenAnyValue(x => x.FilePath, filePath => !string.IsNullOrEmpty(filePath));

        var canDownload = hasDownloadSelection.Zip(hasFilePath, (selection, filepath) => selection && filepath);

        StartDownload = ReactiveCommand.CreateFromTask(() => StartDownloadFn(), canDownload);

        // When Contacts is changed, disable message history, but preserve the previous value
        this.WhenAnyValue(x => x.Contacts).Subscribe(x =>
        {
            if (!x)
            {
                PreviousMessageHistory = MessageHistory;
                MessageHistory = false;
            }
            else
            {
                MessageHistory = PreviousMessageHistory;
            }
        });

        // When either the inventory worlds checkbox changes or the requiredbytes propert of our group list changes, update the bytes total
        // This too can probably be improved.
        var byteTotal = this.
            WhenAnyValue(x => x.InventoryWorlds, x => x.GroupsList.RequiredBytes)
            .Select((p) =>
            {
                var bytes = p.Item1 ? UserStorage.UsedBytes : 0;
                bytes += p.Item2;
                return bytes;
            });

        // Leave the bytes as a number
        byteTotal.ToPropertyEx(this, x => x.RequiredBytes);

        // bytes > 0 => true
        byteTotal.Select(x => x > 0).ToPropertyEx(this, x => x.ShouldShowRequiredBytes);

        //TODO: errors
        _ = LoadGroups();
    }

    private async Task LoadGroups()
    {
        var groups = await GroupService.GetGroups();
        GroupsList.AddGroups(groups);
    }

    private async Task StartDownloadFn()
    {
        await Router.Navigate.Execute(new ProgressViewModel(this));
    }

    private async Task PickDownloadFolder()
    {
        var fileName = await ShowOpenFolderDialog.Handle(new FolderPickerOpenOptions { AllowMultiple=false, Title= Res.WhereDownload});

        if (fileName.HasResult)
        {
            // Suffix the returned path with NeosDownload, this ensures we send stuff to its own folder
            // Before we had this, it was possible for some users to accidentally dump the Neos Download into their Documents folder.
            // This is due to the Download using multiple folders for each user or the assets downloaded.
            // So if a user accidentally selects Documents, for example, then:
            // Documents\U-User & Documents\Assets would be created.

            // That could be confusing, so we suffix our own folder to it which would mean:
            // Documents\NeosDownload\U-User & Documents\NeosDownload\Assets would be created
            // This keeps the files in one folder no matter what people do

            // It could lead to some confusion if people already understand this process and create their own folder for it
            // E.g. Documents\NeosBackup(user made)\NeosDownload(we made this).
            // But user's can always edit the path after selection

            // So anyway, suffix the returned path with NeosDownload
            // See: https://stackoverflow.com/questions/372865/path-combine-for-urls
            // This is so complicated why?
            var uri = fileName.Result!.Append("/NeosDownload");

            FilePath = uri.LocalPath;
        }
        else
        {
            await GlobalInteractions.ShowMessageBox.Handle(new MessageBoxRequest(fileName.Error!));
        }
    }

    public override string ToString()
    {
        return $"Contacts:{Contacts}, MessageHistory:{MessageHistory}, InventoryWorlds:{InventoryWorlds}, CloudVariableDefinitions:{CloudVariableDefinitions}, CloudVariableValues:{CloudVariableValues}, Groups:{GroupsList}";
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Threading.Tasks;
using AccountDownloader.ViewModels;
using Avalonia.Platform.Storage;
using System;

namespace AccountDownloader.Views;

public partial class DownloadSelectionView : ReactiveUserControl<DownloadSelectionViewModel>
{
    public DownloadSelectionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.WhenActivated(d =>
        {
            //In the designer the VM is null, in the designer we don't need to register this too.
            if (ViewModel == null)
                return;

            d(ViewModel.ShowOpenFolderDialog.RegisterHandler(ShowOpenFolderDialog));
        });
        AvaloniaXamlLoader.Load(this);
    }

    private async Task ShowOpenFolderDialog(InteractionContext<FolderPickerOpenOptions, InteractionResult<Uri>> interaction)
    {
        Window? w = null;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            w = desktop.MainWindow;
        }

        if (w == null)
        {
            interaction.SetOutput(InteractionResult<Uri>.WithError(Res.Errors_NoMainWindow));
            return;
        }

        if (!w.StorageProvider.CanPickFolder)
        {
            interaction.SetOutput(InteractionResult<Uri>.WithError(Res.Errors_CannotSelectFolder));
            return;
        }

        var folders = await w.StorageProvider.OpenFolderPickerAsync(interaction.Input);
        if (folders.Count == 0)
        {
            interaction.SetOutput(InteractionResult<Uri>.WithError(Res.Errors_NoFolderSelected));
            return;
        }

        // We don't allow multiple folders so picking the first should be fine for now, but we'll probably need to re-write this if multiple are allowed
        // Paths come in as full URI's this often includes "file:///" we don't need that LocalPath fixes this.
        // Local Path contains an "Operating System" style path which doesn't mangle the URI with URI encoding e.g. %20
        interaction.SetOutput(InteractionResult<Uri>.WithResult(folders[0].Path));
    }
}


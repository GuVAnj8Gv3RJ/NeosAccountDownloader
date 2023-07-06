using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using AccountDownloader.ViewModels;
using MsBox.Avalonia;
using System.Reactive;
using AccountDownloader.Utilities;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Splat;
using AccountDownloader.Services;
using AccountDownloader.Views;
using MessageBox.Avalonia.Models;
using Avalonia.Media.Imaging;
using Avalonia;
using MsBox.Avalonia.Dto;

namespace AccountDownloader.Views
{

    // Making our own enum here to avoid coupling with MessageBox.Avalonia which we might ditch later
    enum BoxType
    {
        Error = 0,
        Info
    }

    // Again our own enum because I really dislike MessageBox.Avalonia
    public enum YesNo
    {
        No = 0,
        Yes
    }

    public partial class MainWindowView : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindowView()
        {
            this.WhenActivated(d =>
            {
                d.Add(GlobalInteractions.ShowError.RegisterHandler(ShowErrorBox));
                d.Add(GlobalInteractions.ShowMessageBox.RegisterHandler(ShowInfoBox));
                d.Add(GlobalInteractions.ShowYesNoBox.RegisterHandler(ShowYesNoBox));
                d.Add(GlobalInteractions.OpenLogLocation.RegisterHandler(OpenLogLocation));
                d.Add(GlobalInteractions.ShowAboutWindow.RegisterHandler(ShowAboutWindow));
            });
            
#if DEBUG
            this.AttachDevTools();
#endif
            AvaloniaXamlLoader.Load(this);

            this.Closing += MainWindow_Closing;
        }

        private async Task OpenLogLocation(InteractionContext<Unit, Unit> obj)
        {
            obj.SetOutput(Unit.Default);
            
            var config = Locator.Current.GetService<Config>();
            
            if (config == null)
            {
                await GlobalInteractions.ShowError.Handle(new MessageBoxRequest("Cannot find Log folder."));
                
                return;
            }
            var res = IOService.OpenFolderDialog(config.LogFolder);
            if (!res.Success)
                await GlobalInteractions.ShowError.Handle(new MessageBoxRequest(res.Error!));
        }

        // This is a little complicated, but I wanted a method that:
        // - Let multiple things "cancel" the window closing
        // - Let them do async things to decide if they should cancel
        // - Attempt to not break the MVVM pattern (where we'd just add multiple event handlers to the Window object)
        // - This works, we can re-write it later if you hate it.
        private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            // Immediately cancel the closing event.
            e.Cancel = true;

            // Ask the interaction system if anyone would like to keep it canceled.
            try
            {
                var result = await GlobalInteractions.OnMainWindowClose.Handle(e);

                // If any interaction handler says "Yes"(true)/cancel, then return
                if (result)
                    return;

                // Otherwise proceed with closing the window.
                SafelyCloseWindow();
            } 
            catch(UnhandledInteractionException<WindowClosingEventArgs,bool>)
            {
                // If no one has registered a handler at this point, we'll end up here because there are no OnMainWindow close handlers.
                // That's fine it just means no one is interested in cancelling the event, so we can just proceed to close
                SafelyCloseWindow();
            }
        }

        private void SafelyCloseWindow()
        {
            //De-register the event, to prevent an infinite loop
            this.Closing -= MainWindow_Closing;

            //Actually close the window.
            this.Close();
        }

        private async Task ShowInfoBox(InteractionContext<MessageBoxRequest, Unit> message)
        {
            await ShowBox(message, BoxType.Info);
        }
        private async Task ShowErrorBox(InteractionContext<MessageBoxRequest, Unit> message)
        {
            await ShowBox(message, BoxType.Error);
        }

        private async Task ShowBox(InteractionContext<MessageBoxRequest, Unit> message, BoxType type)
        {
            // Always, set the output before we show the box.
            message.SetOutput(Unit.Default);
            Bitmap icon = type switch
            {
                BoxType.Error => AssetHelper.GetBitmap("Error.png"),
                _ => AssetHelper.GetBitmap("Information.png"),
            };
            var box = MessageBoxManager.GetMessageBoxCustom(
               new MessageBoxCustomParams
               {
                   ContentTitle = message.Input.Title ?? (type == BoxType.Error ? Res.Error: Res.Information),
                   ImageIcon = icon,
                   ContentMessage = message.Input.Message ?? Res.Errors_UnknownError,
                   WindowStartupLocation = WindowStartupLocation.CenterOwner,
                   WindowIcon = new WindowIcon(AssetHelper.GetBitmap("AppIcon.ico")),
                   ButtonDefinitions = new[]
                   {
                       new ButtonDefinition{Name = Res.OK, IsDefault = true},
                   }
               });
            await box.ShowWindowDialogAsync(this);
        }

        private async Task ShowYesNoBox(InteractionContext<MessageBoxRequest, InteractionResult<YesNo>> message)
        {
            var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams()
            {
                ContentTitle = message.Input.Title ?? Res.AreYouSure,
                ContentMessage = message.Input.Message,
                ImageIcon = AssetHelper.GetBitmap("Question.png"),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowIcon = new WindowIcon(AssetHelper.GetBitmap("AppIcon.ico")),
                // Ideally MessageBox.Avalonia would allow you to specify a return value for the "ButtonDefinition" type, but it doesn't, it returns the string they clicked on
                // TODO: Fix this
                ButtonDefinitions = new[]
                {
                    new ButtonDefinition{ Name=Res.Yes},
                    new ButtonDefinition{ Name=Res.No, IsCancel=true},
                }
            });

            var result = await messageBoxStandardWindow.ShowWindowDialogAsync(this);
            YesNo res = result == Res.Yes ? YesNo.Yes : YesNo.No;

            message.SetOutput(InteractionResult<YesNo>.WithResult(res));
        }

        public async Task ShowAboutWindow(InteractionContext<Unit, Unit> obj)
        {
            obj.SetOutput(Unit.Default);

            var about = new AboutWindowView
            {
                DataContext = new AboutWindowViewModel(),
            };
            await about.ShowDialog(this);
        }
    }
}

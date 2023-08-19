using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AccountDownloader.Services;
using AccountDownloader.Utilities;
using ReactiveUI;
using Splat;

namespace AccountDownloader.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    // The Router associated with this Screen.
    // Required by the IScreen interface.
    public RoutingState Router { get; }

    public ReactiveCommand<Unit, Unit> OpenLogFolder { get; set; }
    public ReactiveCommand<Unit, Unit> ShowAbout { get; set; }

    public ReactiveCommand<string, Unit> SetLanguageCommand { get; }

    private ILocaleService LocaleService { get; }

    public IEnumerable<MenuItemViewModel> MenuItems { get; }
    public MainWindowViewModel()
    {
        Router = new RoutingState();

        LocaleService = Locator.Current.GetService<ILocaleService>() ?? throw new NullReferenceException("No available Locale Service");

        // Register this view model with the DI container, this way we aren't passing around the main router over and over.
        Locator.CurrentMutable.RegisterConstant(this, typeof(IScreen));
        Locator.CurrentMutable.RegisterConstant(this, typeof(MainWindowViewModel));

        OpenLogFolder = ReactiveCommand.CreateFromTask(() => OpenLogFolderFn());
        ShowAbout = ReactiveCommand.CreateFromTask(() => ShowAboutFn());

        SetLanguageCommand = ReactiveCommand.Create<string>((code) => LocaleService.SetLanguage(code));

        // Was having difficulty with dynamic Menus/XAML Looping, so we construct the whole menu here. 
        // See https://docs.avaloniaui.net/docs/controls/menu#dynamically-creating-menus
        // TODO: This kinda breaks MVVM, fix
        var localeItems = LocaleService.AvailableLocales.Select(item => new MenuItemViewModel { Header = item.Name, Command = SetLanguageCommand, CommandParameter = item.Code }).ToList();
        MenuItems = new[]
        {
            new MenuItemViewModel
            {
                Header= Res.Help,
                Items = new[]
                {
                    new MenuItemViewModel { Header=Res.About, Command=ShowAbout},
                    new MenuItemViewModel { Header=Res.OpenLogFolder, Command=OpenLogFolder}
                }
            },
            new MenuItemViewModel
            {
                Header= Res.Settings,
                Items = new[]
                {
                    new MenuItemViewModel { Header= Res.Language, Items= localeItems }
                }
            }
        };

        Router.Navigate.Execute(new GettingStartedViewModel());

        LocaleService.LocaleChanged += LocaleService_LocaleChanged;
    }

    private void LocaleService_LocaleChanged(System.Globalization.CultureInfo obj)
    {
        var currentModel = Router.GetCurrentViewModel();
        if (currentModel != null)
            Router.Navigate.Execute(currentModel);
    }

    private async Task ShowAboutFn()
    {
        await GlobalInteractions.ShowAboutWindow.Handle(Unit.Default);
    }

    public async Task OpenLogFolderFn()
    {
        var config = Locator.Current.GetService<Config>();

        if (config == null)
        {
            await GlobalInteractions.ShowError.Handle(new MessageBoxRequest("Cannot find Log folder."));

            return;
        }
        await GlobalInteractions.OpenFolderLocation.Handle(config.LogFolder);
    }
}

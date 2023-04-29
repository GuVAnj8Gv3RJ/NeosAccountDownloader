using ReactiveUI;
using Splat;
using System;

namespace AccountDownloader.ViewModels;

public class ViewModelBase : ReactiveObject, IRoutableViewModel
{
    // Reference to IScreen that owns the routable view model.
    public IScreen HostScreen => MainWindow;

    public MainWindowViewModel MainWindow { get; }
    public RoutingState Router { get; }

    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

    public ViewModelBase()
    {
        MainWindow = Locator.Current.GetService<MainWindowViewModel>() ?? throw new ArgumentNullException("Could not find main window");
        Router = HostScreen.Router;
    }
}

using System.Reactive;
using ReactiveUI;

namespace AccountDownloader.ViewModels;

public class GettingStartedViewModel : ViewModelBase, IRoutableViewModel
{
    public ReactiveCommand<Unit, IRoutableViewModel> Login { get; }
    public GettingStartedViewModel()
    {
        Login = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new LoginViewModel()));
    }
}

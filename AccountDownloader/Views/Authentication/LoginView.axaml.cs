using Avalonia.ReactiveUI;
using AccountDownloader.ViewModels;


namespace AccountDownloader.Views;

public partial class LoginView : ReactiveUserControl<LoginViewModel>
{
    public LoginView()
    {
        InitializeComponent();
    }
}


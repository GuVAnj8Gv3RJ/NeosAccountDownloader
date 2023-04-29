using System;
using System.Reactive;
using AccountDownloader.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using Splat;
using System.Reactive.Linq;
using AccountDownloader.Utilities;

namespace AccountDownloader.ViewModels;

public class LoginViewModel : ViewModelBase, IValidatableViewModel
{
    [Reactive]
    public string Username { get; set; } = string.Empty;

    [Reactive]
    public string Password { get; set; } = string.Empty;

    public ValidationContext ValidationContext { get; } = new ValidationContext();

    public ReactiveCommand<Unit, AuthResult> Login { get; set; }

    private readonly IAppCloudService CloudService;
    public LoginViewModel()
    {
        CloudService = Locator.Current.GetService<IAppCloudService>() ?? throw new ArgumentNullException("Cannot login without an app service");

        Login = ReactiveCommand.CreateFromTask(() => CloudService.Login(Username, Password), this.IsValid());
        Login.Subscribe(async result =>
        {
            // TOTP Required, go there.
            if (result.state == AuthenticationState.TOTPRequired)
                await Router.Navigate.Execute(new MultiFactorAuthViewModel());
            // Authenticated, no TOTP, go to next
            else if (result.state == AuthenticationState.Authenticated)
                await Router.Navigate.Execute(new DownloadSelectionViewModel());
            // Error, show it
            else 
                await GlobalInteractions.ShowError.Handle(new MessageBoxRequest(result.error ?? Res.Errors_UnexpectedLoginError));
        });

        this.ValidationRule(viewModel => viewModel.Username, username => !string.IsNullOrWhiteSpace(username ?? null), Res.Errors_BlankUsername);
        this.ValidationRule(viewModel => viewModel.Password, password => !string.IsNullOrWhiteSpace(password ?? null), Res.Errors_BlankPassword);
    }
}

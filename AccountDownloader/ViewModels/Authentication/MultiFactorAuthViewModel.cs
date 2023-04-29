using System;
using System.Reactive;
using AccountDownloader.Services;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using Splat;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using System.Reactive.Linq;

namespace AccountDownloader.ViewModels;

public class MultiFactorAuthViewModel : ViewModelBase, IValidatableViewModel
{
    [Reactive]
    public string? TOTPToken { get; set; } = "";

    private IAppCloudService CloudService;

    public ReactiveCommand<Unit, AuthResult> SubmitTOTP { get; set; }

    public ValidationContext ValidationContext { get; } = new ValidationContext();

    public Interaction<string?, Unit> ShowError { get; }

    public MultiFactorAuthViewModel()
    {
        CloudService = Locator.Current.GetService<IAppCloudService>() ?? throw new ArgumentNullException("Cannot login without an app service");

        ShowError = new Interaction<string?, Unit>();

        SubmitTOTP = ReactiveCommand.CreateFromTask(() => CloudService.SubmitTOTP(TOTPToken), this.IsValid());

        this.ValidationRule(viewModel => viewModel.TOTPToken, token => !string.IsNullOrWhiteSpace(token ?? null), "TOTP Token cannot be blank");

        SubmitTOTP.Subscribe(async result =>
        {
            if (result.state == AuthenticationState.TOTPRequired)
            {
                await ShowError.Handle("Invalid TOTP Token");
                return;
            }
            if (result.state == AuthenticationState.Authenticated)
            {
                await Router.Navigate.Execute(new DownloadSelectionViewModel());
                return;
            }

            await ShowError.Handle(result.error);
        });
    }
}

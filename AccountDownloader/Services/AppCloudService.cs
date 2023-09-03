

using System;
using System.Threading.Tasks;
using BaseX;
using CloudX.Shared;
using Microsoft.Extensions.Logging;

namespace AccountDownloader.Services;
public class AppCloudService : IAppCloudService
{
    private string _login = string.Empty;
    private string _password = string.Empty;
    private readonly string _id;


    private readonly CloudXInterface _interface;
    private readonly ILogger logger;

    public AppCloudService(CloudXInterface? cloudInterface, ILogger? logger)
    {
        _id = Guid.NewGuid().ToString();
        _interface = cloudInterface ?? throw new NullReferenceException("Cannot run without a CloudX Interface");
        this.logger = logger ?? throw new NullReferenceException("Cannot run without a Logger");

        UniLog.OnLog += UniLog_OnLog;

        _interface.UserUpdated += Profile.UpdateUser;
    }

    // Lets us see inside cloudx
    private void UniLog_OnLog(string obj)
    {
        this.logger.LogDebug(obj);
    }

    public AuthenticationState AuthState { get; private set; }

    public IUserProfile Profile { get; private set; } = new AppCloudUserProfile();

    public User User { get => _interface.CurrentUser; }

    public async Task<AuthResult> Login(string login, string password)
    {
        this.logger.LogInformation("Logging in user: {user}", login);
        var loginResult = await _interface.Login(login, password, null, _id, false, null, null).ConfigureAwait(false);
        _login = login;
        _password = password;
        return ProcessLoginResult(loginResult);
    }

    public async Task<AuthResult> Logout()
    {
        this.logger.LogInformation("Logging out user: {user}", _login);
        _login = string.Empty; 
        _password = string.Empty;

        await _interface.Logout(true);

        AuthState = AuthenticationState.Unauthenticated;
        return new AuthResult(AuthenticationState.Unauthenticated, null);
    }

    private void PostLogin()
    {
        // Force an update to the user's information, this will get user profile data and capitalization correct
        // This get's called when the interface logs in but we have no real way to know when that happens. As such we'll do it again.
        //await _interface.UpdateCurrentUserInfo();

        // Flash the profile with the new data
        Profile.UpdateUser(_interface.CurrentUser);
    }

    private AuthResult ProcessLoginResult(CloudResult<UserSession>? loginResult)
    {
        AuthResult authResult;
        if (loginResult == null)
        {
            this.logger.LogWarning("Failed to login {user}, with reason {reason}", _login, "CloudX returned null from a login request");
            authResult = new AuthResult(AuthenticationState.Error, "CloudX returned null from a login request");
        } else
        {
            if (loginResult.IsOK)
            {
                authResult = new AuthResult(AuthenticationState.Authenticated, null);
                this.logger.LogInformation("{user} Logged in successfully.", _login);
                PostLogin();
            }
            else
            {
                if (loginResult.Content == "TOTP")
                {
                    this.logger.LogInformation("TOTP Challenge");
                    authResult = new AuthResult(AuthenticationState.TOTPRequired, "TOTP Required");
                }
                else
                {
                    this.logger.LogWarning("Failed to login {user}, with reason {reason}", _login, loginResult.Content);
                    authResult = new AuthResult(AuthenticationState.Error, loginResult.Content);
                }

            }
        }
        
        AuthState = authResult.state;
        return authResult;
    }

    public async Task<AuthResult> SubmitTOTP(string code)
    {
        this.logger.LogInformation("{user} responded to TOTP Challenge",_login);
        var loginResult = await _interface.Login(_login, _password, null, _id, false, null, code).ConfigureAwait(false);
        this.logger.LogDebug("Returned from TOTP Login");
        return ProcessLoginResult(loginResult);
    }
}



using System;
using AccountDownloader.Utilities;
using CloudX.Shared;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace AccountDownloader.Services;

public class AppCloudUserProfile : ReactiveObject, IUserProfile
{

    [Reactive]
    public string UserName { get; set; } = "Unauthenticated";

    [Reactive]
    public Uri PictureURI { get; set; } = AssetHelper.GetUri("AnonymousHeadset.png");

    private static ILogger? Logger;

    public AppCloudUserProfile(User user)
    {
        UpdateUser(user);
        SetupLogger();
    }

    private void SetupLogger()
    {
        Logger = Locator.Current.GetService<ILogger>();
    }

    public AppCloudUserProfile()
    {
        SetupLogger();
    }

    private static Uri? GetProfilePictureUri(string? profilePictureUri)
    {
        Uri uri;
        var success = Uri.TryCreate(profilePictureUri, UriKind.Absolute, out uri!);

        if (!success)
            return null;

        return CloudXInterface.NeosDBToHttp(uri, NeosDB_Endpoint.Default);
    }

    public void UpdateUser(User? user)
    {
        Logger.LogDebug($"Updating user New:{user?.Username}, Old:{UserName} ");
        if (user == null)
            return;

        UserName = user.Username;

        PictureURI = GetProfilePictureUri(user?.Profile?.IconUrl) ?? AssetHelper.GetUri("AnonymousHeadset.png");
    }
}

using AccountDownloader.Models;
using AccountDownloader.Services.Interfaces;
using System.Threading.Tasks;
using System.Text.Json;

namespace AccountDownloader.Services.Windows;


// This uses the Data Protection API's in .NET see: https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-use-data-protection?redirectedfrom=MSDN
// It doesn't work on Linux for that see the linux version of this service.
// https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-7.0
public class WindowsSettingsSerivce : ISettingsService
{
    private ILogger Logger;
    public Settings Settings { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public Task<Settings> LoadSettings()
    {
        throw new System.NotImplementedException();
    }

    public Task SaveSettings()
    {
        
    }

    public WindowsSettingsSerivce(ILogger logger)
    {
        Logger = logger;
    }
}

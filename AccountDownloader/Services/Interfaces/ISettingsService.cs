using AccountDownloader.Models;
using System.Threading.Tasks;

namespace AccountDownloader.Services.Interfaces;

public interface ISettingsService
{
    public Task<Settings> LoadSettings();
    public Task SaveSettings();

    public Settings Settings { get; set; }
}

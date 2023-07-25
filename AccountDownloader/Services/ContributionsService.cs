using System.Collections.Generic;
using System.Text.Json;

using Avalonia.Platform;

using AccountDownloader.Utilities;
using AccountDownloader.Models;

namespace AccountDownloader.Services
{

    public class ContributionsService
    {
        public List<Contributor>? Contributors => file?.Contributors;
        private ContributorsFile? file;

        public ContributionsService()
        {
           file = JsonSerializer.Deserialize<ContributorsFile>(AssetLoader.Open(AssetHelper.GetUri(".all-contributorsrc")), SourceGenerationContext.Default.ContributorsFile);
        }
    }
}

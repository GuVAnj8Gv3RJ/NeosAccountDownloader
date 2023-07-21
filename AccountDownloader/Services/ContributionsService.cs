using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using AccountDownloader.Utilities;
using Avalonia.Platform;

namespace AccountDownloader.Services
{
    public class ContributorsFile
    {
        [JsonPropertyName("contributors")]
        public List<Contributor> Contributors { get; set; }
    }
    public class Contributor {
        [JsonPropertyName ("name")]
        public string Name { get; set; }

        [JsonPropertyName ("contributions")]
        public List<string> Contributions { get; set; }

        [JsonPropertyName("avatar_url")]
        public string Avatar { get; set; }
    }
    public class ContributionsService
    {
        public List<Contributor>? Contributors => file?.Contributors;
        private ContributorsFile? file;

        public ContributionsService()
        {
            //TODO: I have no idea if this will work
            file = JsonSerializer.Deserialize<ContributorsFile>(AssetLoader.Open(AssetHelper.GetUri(".all-contributorsrc")));
        }
    }
}

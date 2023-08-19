using AccountDownloader.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text.Json.Serialization;

namespace AccountDownloader.Models
{
    public class Settings
    {
        [JsonPropertyName("token")]
        public SecureString? SessionToken { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("config")]
        public JSONAccountDownloadConfig? DownloadConfiguration { get; set; }
    }

    public class JSONAccountDownloadConfig : IAccountDownloadConfig
    {
        public bool UserMetadata { get; set; }

        public bool Contacts { get; set; }

        public bool MessageHistory { get; set; }

        public bool InventoryWorlds { get; set; }

        public bool CloudVariableDefinitions { get; set; }

        public bool CloudVariableValues {get; set;}
        public string FilePath { get; set; } = "";

        public IEnumerable<string> Groups { get; set; } = new List<string>();



        // TODO: collapse interfaces here to avoid weirdness
        public IObservable<IReactivePropertyChangedEventArgs<IReactiveObject>> Changing => throw new NotImplementedException();

        public IObservable<IReactivePropertyChangedEventArgs<IReactiveObject>> Changed => throw new NotImplementedException();


        public IDisposable SuppressChangeNotifications()
        {
            throw new NotImplementedException();
        }
    }
}

using Avalonia.Platform;
using Avalonia;
using System;
using System.Reflection;
using Avalonia.Media.Imaging;

namespace AccountDownloader.Utilities
{
    public class AssetHelper
    {
        public static string GetAssetBasePath()
        {
            var aName = Assembly.GetEntryAssembly()!.GetName().Name;

            return $"avares://{aName}/Assets";
        }

        public static Bitmap GetBitmap(string input)
        {
            var uri = GetUri(input);
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var asset = assets!.Open(uri);

            return new Bitmap(asset);
        }

        internal static Uri GetUri(string input)
        {
            return new Uri(GetAssetBasePath() + $"/{input}");
        }
    }
}

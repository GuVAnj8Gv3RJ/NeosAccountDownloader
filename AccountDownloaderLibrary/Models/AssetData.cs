namespace AccountDownloaderLibrary
{
    public readonly struct AssetData : IDisposable
    {
        public readonly Uri url;
        public readonly Stream stream;

        public AssetData(Uri url)
        {
            this.url = url;
            this.stream = null;
        }

        public AssetData(Stream stream)
        {
            this.stream = stream;
            this.url = null;
        }

        public void Dispose()
        {
            stream?.Dispose();
        }

        public static implicit operator AssetData(Stream stream) => new(stream);
        public static implicit operator AssetData(Uri url) => new(url);
    }
}


namespace AccountDownloaderLibrary
{
    // From: https://stackoverflow.com/a/66270371/2095344
    public static class HttpClientExtensions
    {
        public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string fileName)
        {
            using (var s = await client.GetStreamAsync(uri))
            {   
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    await s.CopyToAsync(fs);
                }
            }
        }
    }
}

using AccountDownloaderLibrary.Mime;

namespace AccountDownloaderLibrary
{
    // From: https://stackoverflow.com/a/66270371/2095344
    public static class HttpClientExtensions
    {
        // TODO: now that this does some custom logic (figures out the extension of the file), this should NOT be an extension method.
        // It is specialized for files from Neos.
        // Move it to its own class.
        public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string FileName)
        {
            using (var s = await client.GetStreamAsync(uri))
            {   
                using (var fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    await s.CopyToAsync(fs);
                }
            }

            // TODO: I hate that we move after downloading.
            // MimeDetective does have a stream reading overload. When It is used, it seems to "corrupt" or halt the stream
            // The resultant file then has 0 bytes.
            // I don't understand streams enough to figure this out, but I would love to save the file operations here.
            //
            // So ideally:
            // 1. Get the HTTP Stream
            // 2. Examine the HTTP Stream for file extension
            // 3. Create FileStream with file extension
            // 4. CopyToAsync the stream
            //
            // At least the file move should be async before this is released.
            // #HelpWanted

            var extension = MimeDetector.Instance.MostLikelyFileExtension(FileName);
            File.Move(FileName, FileName + "." + extension);

        }
    }
}

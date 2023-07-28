// See https://aka.ms/new-console-template for more information
using AccountDownloaderLibrary;

// Use this to test the extension stuff in DownloadFileTaskAsync
// See the note there for why this exists.

//TODO: this is broken atm, but I'll leave it here for testing.
Console.WriteLine("Hello, World!");

var client = new HttpClient();

var assetID = "REDACTED";

await client.DownloadFileTaskAsync(new Uri("https://assets.neos.com/assets/" + assetID), "test");


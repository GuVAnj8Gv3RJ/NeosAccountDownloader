using AccountDownloaderLibrary.Interfaces;

namespace AccountDownloaderLibrary;

#nullable enable

// Some errors might not generate a full exception, but some might. This lets us wrap both.
// Generically use Error for Message boxes and the full exception for logging.
public class DownloadResult : IDownloadResult
{
    public string? Error { get; }

    public DownloadResultType Result { get; }

    public Exception? Exception { get; }

    public DownloadResult(DownloadResultType result, string? error = null, Exception? exception = null)
    {
        Result = result;
        Error = error;
        Exception = exception;
    }

    public static DownloadResult Cancelled => new DownloadResult(DownloadResultType.Cancelled);
    public static DownloadResult Successful => new DownloadResult(DownloadResultType.Sucessful);
}

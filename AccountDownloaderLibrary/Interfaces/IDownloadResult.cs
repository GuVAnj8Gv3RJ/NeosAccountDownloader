namespace AccountDownloaderLibrary.Interfaces;

public enum DownloadResultType
{
    Error = 0,
    Cancelled,
    Sucessful,
}

public interface IDownloadResult
{
    public DownloadResultType Result { get; }
    public string? Error { get; }

    // If there's an exception, there may be more details here
    public Exception? Exception { get; }
}

using System.ComponentModel;

namespace AccountDownloaderLibrary
{
    public partial class VariableDownloadStatus: INotifyPropertyChanged
    {
        public int DownloadedVariableCount { get; set; } = 0;
        public int DownloadedVariableDefinitionCount { get; set; } = 0;

        public string GenerateReport()
        {
            return $"Downloaded Definitions: {DownloadedVariableDefinitionCount}\nDownloaded Variable Value Count: {DownloadedVariableCount}";
        }
    }
}

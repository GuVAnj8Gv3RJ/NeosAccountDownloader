using System.ComponentModel;

namespace AccountDownloaderLibrary
{
    // Fody makes this great: https://github.com/Fody/PropertyChanged#code-generator
    /// <summary>
    /// Represents current status of a group to download
    /// </summary>
    public partial class GroupDownloadStatus: INotifyPropertyChanged
    {
        /// <summary>
        /// OwnerID of the group
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Name of the group
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Were members already downloaded? It doesn't make sense to have progress indicator for this,
        /// because there's barely any data to go with this.
        /// </summary>
        public int DownloadedMemberCount { get; set; }

        /// <summary>
        /// Status of the downloaded records
        /// </summary>
        public RecordDownloadStatus RecordsStatus { get; set; } = new RecordDownloadStatus();

        /// <summary>
        /// Download status of the variables of the group
        /// </summary>
        public VariableDownloadStatus VariablesStatus { get; set; } = new VariableDownloadStatus();
    }
}

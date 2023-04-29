using System.ComponentModel;
using System.Text;

namespace AccountDownloaderLibrary
{
    public class RecordDownloadFailure
    {
        public string RecordId { get; set; } = string.Empty;

        public string OwnerId { get; set; } = string.Empty;
        public string RecordName { get; set; } = string.Empty;

        public string RecordPath { get; set; } = string.Empty;

        public string FailureReason { get; set; } = string.Empty;
    }

    // Fody is making this magical see: https://github.com/Fody/PropertyChanged#code-generator
    public partial class RecordDownloadStatus: INotifyPropertyChanged
    {
        /// <summary>
        /// Total number of records to be downloaded
        /// </summary>
        public int TotalRecordCount { get; set; } = 0;

        /// <summary>
        /// Total number of records that were downloaded
        /// </summary>
        public int DownloadedRecordCount { get; set; } = 0;

        /// <summary>
        /// How many bytes need to be uploaded
        /// </summary>
        public long BytesToUpload { get; set; } = 0;

        /// <summary>
        /// How many bytes were already uploaded
        /// </summary>
        public long BytesUploaded { get; set; } = 0;

        /// <summary>
        /// How many assets need to be uplaoded
        /// </summary>
        public int AssetsToUpload { get; set; } = 0;

        /// <summary>
        /// How many assets were uploaded
        /// </summary>
        public int AssetsUploaded { get; set; } = 0;

        /// <summary>
        /// Records that failed to download
        /// </summary>
        public List<RecordDownloadFailure> FailedRecords { get; set; } = new List<RecordDownloadFailure>();

        public string GenerateReport()
        {
            var b = new StringBuilder();
            b.AppendLine($"Records: {TotalRecordCount} / {DownloadedRecordCount}");
            b.AppendLine($"Failed: {FailedRecords.Count}");
            foreach (var r in FailedRecords)
            {
                b.AppendLine($"{r.RecordName} failed. Reason: {r.FailureReason}");
            }

            return b.ToString();
        }
    }
}

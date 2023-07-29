using BaseX;
using System.ComponentModel;
using System.Text;

namespace AccountDownloaderLibrary
{
    // Fody is making this magical see: https://github.com/Fody/PropertyChanged#code-generator
    /// <summary>
    /// Status of the download
    /// </summary>
    public partial class AccountDownloadStatus: INotifyPropertyChanged
    {
        public TimeSpan? TotalTime => CompletedOn - StartedOn;

        /// <summary>
        /// Represents the "phase" of the download E.g. Contacts, User Records etc
        /// </summary>
        public string Phase { get; set; } = string.Empty;

        /// <summary>
        /// When was the download started on
        /// </summary>
        public DateTimeOffset? StartedOn { get; set; }

        /// <summary>
        /// When was the download completed
        /// </summary>
        public DateTimeOffset? CompletedOn { get; set; }

        /// <summary>
        /// Download status of the records of the user
        /// </summary>
        public RecordDownloadStatus UserRecordsStatus { get; set; } = new RecordDownloadStatus();

        /// <summary>
        /// Download status of the variables of the user
        /// </summary>
        public VariableDownloadStatus UserVariablesStatus { get; set; } = new VariableDownloadStatus();

        /// <summary>
        /// Average number of records that are downloaded per minute
        /// </summary>
        public double RecordsPerMinute { get; set; } = 0;

        /// <summary>
        /// Name of the entity that's being currently downloaded (either the username or group name)
        /// </summary>
        public string CurrentlyDownloadingName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the individual item that's being currently downloaded
        /// </summary>
        public string CurrentlyDownloadingItem { get; set; } = string.Empty;

        /// <summary>
        /// Total number of contacts (and their messages) to download
        /// </summary>
        public int TotalContactCount { get; set; } = 0;

        /// <summary>
        /// Number of contacts that have already been fully downloaded (including their message history)
        /// </summary>
        public int DownloadedContactCount { get; set; } = 0;

        /// <summary>
        /// Total number of messages that have been already downloaded.
        /// </summary>
        public int DownloadedMessageCount { get; set; } = 0;

        /// <summary>
        /// Number of groups to download for given user
        /// </summary>
        public int TotalGroupCount { get; set; } = 0;

        /// <summary>
        /// Number of groups that were completely downloaded (including their records downloaded)
        /// </summary>
        public int DownloadedGroupCount { get; set; } = 0;

        /// <summary>
        /// Assets that for some reason were missing on the source and could not be uploaded.
        /// Mainly for diagnostic purposes, but could be used in the future to try to relocate those.
        /// </summary>
        public List<AssetFailure> AssetFailures { get; set; } = new List<AssetFailure>();

        /// <summary>
        /// If the download failed
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Download statuses of the groups
        /// </summary>
        public List<GroupDownloadStatus> GroupStatuses { get; set; } = new List<GroupDownloadStatus>();

        public void RegisterAssetFailure(AssetFailure failure)
        {
            lock (AssetFailures)
                AssetFailures.Add(failure);
        }

        public GroupDownloadStatus GetGroupStatus(string ownerId, string groupName)
        {
            var status = GroupStatuses?.FirstOrDefault(s => s.OwnerId == ownerId);

            if (status == null)
            {
                GroupStatuses ??= new List<GroupDownloadStatus>();

                status = new GroupDownloadStatus()
                {
                    OwnerId = ownerId,
                    GroupName = groupName
                };

                GroupStatuses.Add(status);
            }

            return status;
        }

        public int TotalDownloadedMemberCount => GroupStatuses?.Sum(g => g.DownloadedMemberCount) ?? 0;

        public int TotalDownloadedVariableCount => UserVariablesStatus.DownloadedVariableCount
            + GroupStatuses?.Sum(g => g.VariablesStatus.DownloadedVariableCount) ?? 0;

        public int TotalDownloadedVariableDefinitionCount => UserVariablesStatus.DownloadedVariableDefinitionCount
            + GroupStatuses?.Sum(g => g.VariablesStatus.DownloadedVariableDefinitionCount) ?? 0;


        // These next FEW are now full properties due to our need to make them INotifyChanged compatible so Avalonia will re-draw the UI
        public int TotalRecordCount { get; set; } = 0;
        public int TotalDownloadedRecordCount { get; set; } = 0;
        public int TotalAssetCount { get; set; } = 0;
        public int TotalDownloadedAssetCount { get; set; } = 0;

        public int AssetSkipped { get; set; } = 0;

        public int TotalFailedRecordCount
        {
            get
            {
                var count = UserRecordsStatus.FailedRecords?.Count ?? 0;

                if (GroupStatuses != null)
                    count += GroupStatuses.Sum(g => g.RecordsStatus.FailedRecords?.Count ?? 0);

                return count;
            }
        }

        public float PercentageRecords
        {
            get
            {
                return Percentage(TotalRecordCount, TotalDownloadedRecordCount);
            }
        }

        public float PercentageContacts
        {
            get
            {
                return Percentage(TotalContactCount, DownloadedContactCount);
            }
        }

        public float PercentageGroups
        {
            get
            {
                return Percentage(TotalGroupCount, DownloadedGroupCount);
            }
        }

        

        private float Percentage(int current, int total)
        {
            if (total == 0 || current == 0)
                return 0;

            return current / total;
        }

        // Updating stats
        DateTime? _lastSnapshot;
        int _lastDownloadedRecords;

        public void UpdateStats()
        {
            var minutes = (DateTime.UtcNow - _lastSnapshot)?.TotalMinutes;
            bool shouldUpdateRate = minutes >= 1;

            var totalCount = TotalDownloadedRecordCount;

            if (shouldUpdateRate)
            {
                var delta = totalCount - _lastDownloadedRecords;

                RecordsPerMinute = delta / minutes.Value;
            }
            if (shouldUpdateRate || minutes == null)
            {
                _lastDownloadedRecords = totalCount;
                _lastSnapshot = DateTime.UtcNow;
            }

            UpdateTotals();
        }

        private void UpdateTotals()
        {
            // Do this each time.
            //Catering for INotifyPropertyChanged
            // Total Records
            var count = UserRecordsStatus.TotalRecordCount;

            if (GroupStatuses != null)
                count += GroupStatuses.Sum(g => g.RecordsStatus.TotalRecordCount);

            TotalRecordCount = count;

            // Downloaded Records
            count = UserRecordsStatus.DownloadedRecordCount;

            if (GroupStatuses != null)
                count += GroupStatuses.Sum(g => g.RecordsStatus.DownloadedRecordCount);
            TotalDownloadedRecordCount = count;

            // assets
            count = UserRecordsStatus.AssetsUploaded;
            if (GroupStatuses != null)
                count += GroupStatuses.Sum(g => g.RecordsStatus.AssetsUploaded);
            TotalDownloadedAssetCount = count;

            count = UserRecordsStatus.AssetsToUpload;
            if (GroupStatuses != null)
                count += GroupStatuses.Sum(g => g.RecordsStatus.AssetsToUpload);
            TotalAssetCount = count;
        }

        //TODO: I want to move these out of this class
        public string GenerateReport()
        {
            //TODO: some sort of templating library
            var b = new StringBuilder();
            b.AppendLine("Download Report");
            b.AppendLine("----------------------");
            b.AppendLine($"Contacts: {DownloadedContactCount} / {TotalContactCount}");
            b.AppendLine($"Messages: {DownloadedMessageCount}");
            b.AppendLine($"Assets: {TotalDownloadedAssetCount}/ {TotalAssetCount}");
            b.AppendLine(UserVariablesStatus.GenerateReport());
            b.AppendLine(UserRecordsStatus.GenerateReport());

            foreach (var g in GroupStatuses)
            {
                b.AppendLine($"Group: {g.GroupName}");
                b.AppendLine("----------------------");
                b.AppendLine(g.RecordsStatus.GenerateReport());
                b.AppendLine(g.VariablesStatus.GenerateReport());
            }

            // We only need totals if there's at least one group
            if(GroupStatuses.Count > 0)
            {
                b.AppendLine("Record Totals");
                b.AppendLine("----------------------");
                b.AppendLine($"Total records:  {TotalDownloadedRecordCount} / {TotalRecordCount}");
                b.AppendLine($"Total failed records: {TotalFailedRecordCount}");
            }

            // ASSETS
            if (AssetFailures.Count > 0)
            {
                b.AppendLine("Asset Failures");
                b.AppendLine("----------------------");
                b.Append(GenerateAssetFailuresReport());
            }


            if (!string.IsNullOrEmpty(Error))
                b.AppendLine("Error: " + Error);

            return b.ToString();

        }

        private string GenerateAssetFailuresReport()
        {
            var b = new StringBuilder();
            foreach(var failure in AssetFailures)
            {
                b.AppendLine($"{failure.Hash} for {failure.RecordName} at path {failure.RecordPath} owned by {failure.OwnerId} failed due to {failure.Reason}.");
            }
            return b.ToString();
        }
    }
}

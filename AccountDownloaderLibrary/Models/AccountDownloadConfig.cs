namespace AccountDownloaderLibrary
{
    public class AccountDownloadConfig
    {
        /// <summary>
        /// Should user metadata be downloaded?
        /// </summary>
        public bool DownloadUserMetadata { get; set; } = true;

        /// <summary>
        /// Should contacts be downloaded?
        /// </summary>
        public bool DownloadContacts { get; set; } = true;

        /// <summary>
        /// Should contact message history be downloaded? 
        /// </summary>
        public bool DownloadMessageHistory { get; set; } = true;

        /// <summary>
        /// Should cloud variables be downloaded?
        /// </summary>
        public bool DownloadCloudVariables { get; set; } = true;

        /// <summary>
        /// Should cloud variables be downloaded?
        /// </summary>
        public bool DownloadCloudVariableDefinitions { get; set; } = true;

        /// <summary>
        /// Should records beloning to user be downloaded?
        /// </summary>
        public bool DownloadUserRecords { get; set; } = true;

        /// <summary>
        /// Optional list of records to download
        /// </summary>
        public List<string> RecordsToDownload { get; set; }

        /// <summary>
        /// Optional list of variables to download
        /// </summary>
        public List<string> VariablesToDownload { get; set; }

        /// <summary>
        /// Should only the latest records be downloaded?
        /// </summary>
        public bool OnlyNewRecords { get; set; } = false;

        /// <summary>
        /// Force overwrite synced records even if there's a conflict
        /// </summary>
        public bool ForceOverwrite { get; set; }

        /// <summary>
        /// Should groups be downloaded?
        /// </summary>
        public bool DownloadGroups { get; set; } = true;

        /// <summary>
        /// Optional list of groups to download. If empty, all of them will be downloaded.
        /// </summary>
        public HashSet<string> GroupsToDownload { get; set; }

        /// <summary>
        /// Parallelism for download jobs
        /// </summary>
        public int MaxDegreeOfParallelism = 8;
    }
}

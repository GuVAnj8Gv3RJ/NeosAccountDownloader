using System.Collections.Generic;
using System.Collections.ObjectModel;
using AccountDownloaderLibrary;
using ReactiveUI;

namespace AccountDownloader.ViewModels
{
    public class FailedRecordsViewModel : ReactiveObject
    {
        public ObservableCollection<RecordDownloadFailure> FailedRecords {get;}
        public bool ShouldShow { get; }

        public FailedRecordsViewModel(List<RecordDownloadFailure> failures)
        {
            FailedRecords = new ObservableCollection<RecordDownloadFailure>(failures);
            ShouldShow = FailedRecords.Count > 0;
        }
	}
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using AccountDownloaderLibrary;
using ReactiveUI;

namespace AccountDownloader.ViewModels;

// TODO: Collate all failures into a single model, I hated duplicating the work here.
public class FailedRecordsViewModel : ReactiveObject
{
    public ObservableCollection<RecordDownloadFailure> FailedRecords {get;}
    public ObservableCollection<AssetFailure> FailedAssets { get; }
    public bool ShouldShowRecordFailures { get; }
    public bool ShouldShowAssetFailures { get; }

    public FailedRecordsViewModel(List<RecordDownloadFailure> recordFailures, List<AssetFailure> assetFailures)
    {
        FailedRecords = new ObservableCollection<RecordDownloadFailure>(recordFailures);
        FailedAssets = new ObservableCollection<AssetFailure>(assetFailures);
        ShouldShowRecordFailures = FailedRecords.Count > 0;
        ShouldShowAssetFailures = FailedAssets.Count > 0;
    }
}

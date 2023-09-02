using CloudX.Shared;

namespace AccountDownloaderLibrary.Implementations;

public class PaginatedRecordSearch<R> where R : class, IRecord, new()
{
    private SearchParameters searchParameters;

    private CloudXInterface cloud;

    public bool HasMoreResults { get; private set; }
    public int Offset { get; private set; } = 0;

    public PaginatedRecordSearch(SearchParameters searchParameters, CloudXInterface cloud)
    {
        this.searchParameters = searchParameters;
        this.cloud = cloud;
        HasMoreResults = true;
    }

    public async Task<IEnumerable<R>> Next()
    {
        searchParameters.Offset = Offset;
        searchParameters.Count = searchParameters.Count;
        CloudResult<SearchResults<R>> cloudResult = await cloud.FindRecords<R>(searchParameters).ConfigureAwait(continueOnCapturedContext: false);
        if (cloudResult.IsOK)
        {
            Offset += searchParameters.Count;

            cloud.RecordCache<R>().Cache(cloudResult.Entity.Records);

            HasMoreResults = cloudResult.Entity.HasMoreResults;
            return cloudResult.Entity.Records;
        }
        else
        {
            HasMoreResults = false;
            return Enumerable.Empty<R>();
        }
    }
}

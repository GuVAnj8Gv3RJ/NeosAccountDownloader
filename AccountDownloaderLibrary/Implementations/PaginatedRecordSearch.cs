using AccountDownloaderLibrary.Models;
using CloudX.Shared;

namespace AccountDownloaderLibrary.Implementations;

public class PaginatedRecordSearch<R> where R : class, IRecord, new()
{
    private AccountDownloaderSearchParameters searchParameters;

    private CloudXInterface cloud;

    public bool HasMoreResults { get; private set; }
    public int Offset { get; private set; } = 0;

    public PaginatedRecordSearch(AccountDownloaderSearchParameters searchParameters, CloudXInterface cloud)
    {
        this.searchParameters = searchParameters;
        this.cloud = cloud;
        HasMoreResults = true;
    }

    public Task<CloudResult<SearchResults<R>>> FindRecords(AccountDownloaderSearchParameters search)
    {
        return cloud.POST<SearchResults<R>>("api/records/pagedSearch", search);
    }

    public async Task<IEnumerable<R>> Next()
    {
        searchParameters.Offset = Offset;
        searchParameters.Count = searchParameters.Count;
        searchParameters.OnlyFeatured = null;
        CloudResult<SearchResults<R>> cloudResult = await FindRecords(searchParameters).ConfigureAwait(continueOnCapturedContext: false);
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

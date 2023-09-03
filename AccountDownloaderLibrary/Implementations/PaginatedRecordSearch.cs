using AccountDownloaderLibrary.Models;
using CloudX.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountDownloaderLibrary.Implementations;

public class PaginatedRecordSearch<R> where R : class, IRecord, new()
{
    private AccountDownloaderSearchParameters searchParameters;

    private CloudXInterface cloud;

    public bool HasMoreResults { get; private set; }
    public int Offset { get; private set; } = 0;

    private ILogger Logger;

    public PaginatedRecordSearch(AccountDownloaderSearchParameters searchParameters, CloudXInterface cloud, ILogger logger)
    {
        this.searchParameters = searchParameters;
        this.Logger = logger;
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

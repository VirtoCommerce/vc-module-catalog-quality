using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models.Search;
using VirtoCommerce.CatalogQuality.Core.Services;

namespace VirtoCommerce.CatalogQuality.Data.Queries;

public class SearchQualityDataQueryHandler : IQueryHandler<SearchQualityDataQuery, SearchQualityDataResult>
{
    private readonly IQualityDataService _qualityDataService;

    public SearchQualityDataQueryHandler(
        IQualityDataService qualityDataService
        )
    {
        _qualityDataService = qualityDataService;
    }

    public virtual async Task<SearchQualityDataResult> Handle(SearchQualityDataQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var result = await _qualityDataService.SearchAsync(request);

        return result;
    }
}

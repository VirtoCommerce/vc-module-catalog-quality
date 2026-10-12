using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models.Search;
using VirtoCommerce.CatalogQuality.Core.Services;

namespace VirtoCommerce.CatalogQuality.Data.Queries;

public class SearchQualityTargetQueryHandler : IQueryHandler<SearchQualityTargetQuery, SearchQualityTargetResult>
{
    private readonly IQualityTargetSearchService _qualityTargetSearchService;

    public SearchQualityTargetQueryHandler(
        IQualityTargetSearchService qualityTargetSearchService
        )
    {
        _qualityTargetSearchService = qualityTargetSearchService;
    }

    public virtual async Task<SearchQualityTargetResult> Handle(SearchQualityTargetQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var result = await _qualityTargetSearchService.SearchAsync(request);

        return result;
    }
}

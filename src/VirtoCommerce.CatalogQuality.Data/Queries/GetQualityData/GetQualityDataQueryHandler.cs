using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Services;

namespace VirtoCommerce.CatalogQuality.Data.Queries.GetQualityData;

public class GetQualityDataQueryHandler : IQueryHandler<GetQualityDataQuery, QualityData>
{
    private readonly IQualityDataService _qualityDataService;

    public GetQualityDataQueryHandler(
        IQualityDataService qualityDataService
        )
    {
        _qualityDataService = qualityDataService;
    }

    public virtual async Task<QualityData> Handle(GetQualityDataQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.EntityId == null)
        {
            throw new ArgumentNullException(nameof(request.EntityId));
        }

        if (request.EntityType == null)
        {
            throw new ArgumentNullException(nameof(request.EntityType));
        }

        var result = await _qualityDataService.GetQualityDataByEntityAsync(request.EntityId, request.EntityType);

        return result;
    }
}

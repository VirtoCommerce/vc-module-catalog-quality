using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogQuality.Data.Queries;

public class GetQualityTargetQueryHandler : IQueryHandler<GetQualityTargetQuery, QualityTarget>
{
    private readonly IQualityTargetCrudService _qualityTargetCrudService;

    public GetQualityTargetQueryHandler(
        IQualityTargetCrudService qualityTargetCrudService
        )
    {
        _qualityTargetCrudService = qualityTargetCrudService;
    }

    public virtual async Task<QualityTarget> Handle(GetQualityTargetQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Id == null)
        {
            throw new ArgumentNullException(nameof(request.Id));
        }

        var result = await _qualityTargetCrudService.GetByIdAsync(request.Id);

        return result;
    }
}

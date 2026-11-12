using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Services;

namespace VirtoCommerce.CatalogQuality.Data.Queries;

public class GetQualityEntityTypesQueryHandler : IQueryHandler<GetQualityEntityTypesQuery, IList<QualityEntityType>>
{
    private readonly IQualityEntityTypeRegistrar _qualityEntityTypeRegistrar;

    public GetQualityEntityTypesQueryHandler(
        IQualityEntityTypeRegistrar qualityEntityTypeRegistrar
        )
    {
        _qualityEntityTypeRegistrar = qualityEntityTypeRegistrar;
    }

    public virtual Task<IList<QualityEntityType>> Handle(GetQualityEntityTypesQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return Task.FromResult(_qualityEntityTypeRegistrar.GetAll());
    }
}

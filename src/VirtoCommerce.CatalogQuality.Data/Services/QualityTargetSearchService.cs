using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Models.Search;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.CatalogQuality.Data.Models;
using VirtoCommerce.CatalogQuality.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogQuality.Data.Services;

public class QualityTargetSearchService : SearchService<SearchQualityTargetCriteria,
    SearchQualityTargetResult, QualityTarget, QualityTargetEntity>,
    IQualityTargetSearchService
{
    public QualityTargetSearchService(
        Func<ICatalogQualityRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IQualityTargetCrudService crudService,
        IOptions<CrudOptions> crudOptions
        )
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<QualityTargetEntity> BuildQuery(IRepository repository, SearchQualityTargetCriteria criteria)
    {
        var query = ((ICatalogQualityRepository)repository).QualityTargets;

        if (!criteria.ObjectIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
        }

        if (!string.IsNullOrEmpty(criteria.Keyword))
        {
            query = query.Where(x => x.Name.Contains(criteria.Keyword));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(SearchQualityTargetCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos = [
                    new SortInfo
                    {
                        SortColumn = ReflectionUtility.GetPropertyName<QualityTargetEntity>(x => x.Name),
                        SortDirection = SortDirection.Ascending
                    }
                ];
        }

        return sortInfos;
    }
}

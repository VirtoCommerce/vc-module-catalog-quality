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

public class QualityDataSearchService : SearchService<SearchQualityDataCriteria,
    SearchQualityDataResult, QualityData, QualityDataEntity>,
    IQualityDataSearchService
{
    public QualityDataSearchService(
        Func<ICatalogQualityRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IQualityDataCrudService crudService,
        IOptions<CrudOptions> crudOptions
        )
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<QualityDataEntity> BuildQuery(IRepository repository, SearchQualityDataCriteria criteria)
    {
        var query = ((ICatalogQualityRepository)repository).QualityDatas;

        if (!string.IsNullOrEmpty(criteria.Keyword))
        {
            query = query.Where(x => x.EntityId.Contains(criteria.Keyword));
        }

        if (!string.IsNullOrEmpty(criteria.EntityId))
        {
            query = query.Where(x => string.Equals(x.EntityId, criteria.EntityId));
        }

        if (!string.IsNullOrEmpty(criteria.EntityType))
        {
            query = query.Where(x => string.Equals(x.EntityType, criteria.EntityType));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(SearchQualityDataCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;
        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos = [
                    new SortInfo
                    {
                        SortColumn = ReflectionUtility.GetPropertyName<QualityDataEntity>(x => x.CreatedDate),
                        SortDirection = SortDirection.Descending
                    }
                ];
        }

        return sortInfos;
    }
}

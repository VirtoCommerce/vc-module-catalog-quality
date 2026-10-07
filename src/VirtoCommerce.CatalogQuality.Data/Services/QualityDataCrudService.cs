using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.CatalogQuality.Data.Models;
using VirtoCommerce.CatalogQuality.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CatalogQuality.Data.Services;

public class QualityDataCrudService : CrudService<QualityData, QualityDataEntity,
    GenericChangedEntryEvent<QualityData>, GenericChangedEntryEvent<QualityData>>,
    IQualityDataCrudService
{
    public QualityDataCrudService(
        Func<ICatalogQualityRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher
        )
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
    }

    protected override Task<IList<QualityDataEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ICatalogQualityRepository)repository).GetQualityDatasByIdsAsync(ids, responseGroup);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogQuality.Data.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogQuality.Data.Repositories;

public class CatalogQualityRepository : DbContextRepositoryBase<CatalogQualityDbContext>, ICatalogQualityRepository
{
    public CatalogQualityRepository(CatalogQualityDbContext dbContext)
    : base(dbContext)
    {
    }

    public IQueryable<QualityDataEntity> QualityDatas => DbContext.Set<QualityDataEntity>();

    public virtual async Task<IList<QualityDataEntity>> GetQualityDatasByIdsAsync(IList<string> ids, string responseGroup = null)
    {
        var result = Array.Empty<QualityDataEntity>();

        if (!ids.IsNullOrEmpty())
        {
            result = await QualityDatas.Where(x => ids.Contains(x.Id)).ToArrayAsync();
        }
        return result;
    }

    public virtual async Task<QualityDataEntity> GetQualityDataByEntityAsync(string entityId, string entityType)
    {
        QualityDataEntity result = null;

        if (!string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(entityType))
        {
            result = await QualityDatas.FirstOrDefaultAsync(x => x.EntityId == entityId && x.EntityType == entityType);
        }

        return result;
    }

}

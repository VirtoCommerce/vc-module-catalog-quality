using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Data.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogQuality.Data.Repositories;

public interface ICatalogQualityRepository : IRepository
{
    IQueryable<QualityDataEntity> QualityDatas { get; }

    Task<IList<QualityDataEntity>> GetQualityDatasByIdsAsync(IList<string> ids, string responseGroup = null);
    Task<QualityDataEntity> GetQualityDataByEntityAsync(string entityId, string entityType);
}

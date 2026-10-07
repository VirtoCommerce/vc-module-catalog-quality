using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Models.Search;

namespace VirtoCommerce.CatalogQuality.Core.Services;

public interface IQualityDataService
{
    Task<SearchQualityDataResult> SearchAsync(SearchQualityDataCriteria criteria);

    Task<QualityData> GetQualityDataByEntityAsync(string entityId, string entityType);

    Task SaveChangesAsync(IList<QualityData> models);

    Task DeleteAsync(IList<string> ids);
}

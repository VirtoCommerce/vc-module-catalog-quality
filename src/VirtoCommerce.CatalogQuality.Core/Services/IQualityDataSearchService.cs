using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Models.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CatalogQuality.Core.Services;

public interface IQualityDataSearchService : ISearchService<SearchQualityDataCriteria, SearchQualityDataResult, QualityData>
{
}

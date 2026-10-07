using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogQuality.Core.Models.Search;

public class SearchQualityDataCriteria : SearchCriteriaBase
{
    public string EntityId { get; set; }

    public string EntityType { get; set; }
}

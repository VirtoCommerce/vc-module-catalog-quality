using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models;

namespace VirtoCommerce.CatalogQuality.Data.Queries.GetQualityData;

public class GetQualityDataQuery : IQuery<QualityData>
{
    [Required]
    public string EntityId { get; set; }

    [Required]
    public string EntityType { get; set; }
}

using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models;

namespace VirtoCommerce.CatalogQuality.Data.Queries;

public class GetQualityTargetQuery : IQuery<QualityTarget>
{
    [Required]
    public string Id { get; set; }
}

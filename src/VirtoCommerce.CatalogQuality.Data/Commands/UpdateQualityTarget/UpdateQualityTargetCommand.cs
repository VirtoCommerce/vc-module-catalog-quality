using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models;

namespace VirtoCommerce.CatalogQuality.Data.Commands;

public class UpdateQualityTargetCommand : ICommand
{
    public QualityTarget QualityTarget { get; set; }
}

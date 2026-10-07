using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models;

namespace VirtoCommerce.CatalogQuality.Data.Commands;

public class UpdateQualityDataCommand : ICommand
{
    public QualityData QualityData { get; set; }
}

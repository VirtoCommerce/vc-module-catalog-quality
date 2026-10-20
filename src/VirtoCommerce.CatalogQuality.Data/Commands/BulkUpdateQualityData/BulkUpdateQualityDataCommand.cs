using System.Collections.Generic;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models;

namespace VirtoCommerce.CatalogQuality.Data.Commands;

public class BulkUpdateQualityDataCommand : ICommand
{
    public IList<QualityData> QualityDatas { get; set; }
}

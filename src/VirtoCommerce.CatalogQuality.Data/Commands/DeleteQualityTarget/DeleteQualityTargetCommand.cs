using System.Collections.Generic;
using VirtoCommerce.CatalogQuality.Core.Common;

namespace VirtoCommerce.CatalogQuality.Data.Commands;

public class DeleteQualityTargetCommand : ICommand
{
    public IList<string> Ids { get; set; }
}

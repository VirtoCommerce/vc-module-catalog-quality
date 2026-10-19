using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogQuality.Core.Events;

public class TargetChangedEvent : GenericChangedEntryEvent<QualityTarget>
{
    [JsonConstructor]
    public TargetChangedEvent(IEnumerable<GenericChangedEntry<QualityTarget>> changedEntries)
        : base(changedEntries)
    {
    }
}

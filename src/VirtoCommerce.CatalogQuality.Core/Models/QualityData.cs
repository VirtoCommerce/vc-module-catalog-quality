using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogQuality.Core.Models;

public class QualityData : AuditableEntity, ICloneable
{
    public string EntityId { get; set; }
    public string EntityType { get; set; }
    public int ReadinessScore { get; set; }
    public string Suggestions { get; set; }
    public string SuggestionsUrl { get; set; }

    #region ICloneable members
    public virtual object Clone()
    {
        return MemberwiseClone();
    }
    #endregion ICloneable members
}

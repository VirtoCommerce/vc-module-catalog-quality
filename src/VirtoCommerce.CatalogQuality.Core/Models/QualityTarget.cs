using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogQuality.Core.Models;

public class QualityTarget : AuditableEntity, ICloneable
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Prompt { get; set; }

    #region ICloneable members
    public virtual object Clone()
    {
        return MemberwiseClone();
    }
    #endregion ICloneable members
}

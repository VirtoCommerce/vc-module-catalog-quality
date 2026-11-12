namespace VirtoCommerce.CatalogQuality.Core.Models;

/// <summary>
/// An entity type quality targets can be defined for, registered by the module that scores it
/// (see <see cref="Services.IQualityEntityTypeRegistrar"/>).
/// </summary>
public class QualityEntityType
{
    /// <summary>Full type name, as stored in <see cref="QualityTarget.EntityType"/> and <see cref="QualityData.EntityType"/></summary>
    public string Id { get; set; }

    /// <summary>Display name, in English</summary>
    public string Name { get; set; }
}

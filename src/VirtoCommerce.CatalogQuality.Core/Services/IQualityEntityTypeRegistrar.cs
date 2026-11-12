using System.Collections.Generic;
using VirtoCommerce.CatalogQuality.Core.Models;

namespace VirtoCommerce.CatalogQuality.Core.Services;

/// <summary>
/// Entity types quality targets can be defined for. A module that scores its entities registers their type in its
/// <c>PostInitialize</c>; the admin UI lists the registered types.
/// </summary>
public interface IQualityEntityTypeRegistrar
{
    /// <summary>Registers an entity type; registering an id again replaces its name.</summary>
    void Register(QualityEntityType entityType);

    /// <summary>The registered entity types, in registration order.</summary>
    IList<QualityEntityType> GetAll();
}

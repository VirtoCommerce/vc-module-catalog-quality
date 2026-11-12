using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogQuality.Data.Services;

/// <summary>In-memory registry of entity types; registered as a singleton and filled by modules at start-up.</summary>
public class QualityEntityTypeRegistrar : IQualityEntityTypeRegistrar
{
    private readonly List<QualityEntityType> _entityTypes = [];
    private readonly Lock _lock = new();

    public virtual void Register(QualityEntityType entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType.Id, nameof(entityType.Id));
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType.Name, nameof(entityType.Name));

        // a copy, so the caller can't change a registered type afterwards
        var registered = new QualityEntityType { Id = entityType.Id, Name = entityType.Name };

        lock (_lock)
        {
            var index = _entityTypes.FindIndex(x => x.Id.EqualsIgnoreCase(registered.Id));
            if (index >= 0)
            {
                _entityTypes[index] = registered;
            }
            else
            {
                _entityTypes.Add(registered);
            }
        }
    }

    public virtual IList<QualityEntityType> GetAll()
    {
        lock (_lock)
        {
            return _entityTypes.Select(x => new QualityEntityType { Id = x.Id, Name = x.Name }).ToList();
        }
    }
}

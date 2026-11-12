using VirtoCommerce.CatalogQuality.Core.Models;

namespace VirtoCommerce.CatalogQuality.Core.Services;

public static class QualityEntityTypeRegistrarExtensions
{
    /// <summary>Registers an entity type by its id (full type name).</summary>
    public static void Register(this IQualityEntityTypeRegistrar registrar, string id, string name)
    {
        registrar.Register(new QualityEntityType { Id = id, Name = name });
    }

    /// <summary>Registers <typeparamref name="T"/> as an entity type; its id is the type's full name.</summary>
    public static void Register<T>(this IQualityEntityTypeRegistrar registrar, string name)
    {
        registrar.Register(typeof(T).FullName, name);
    }
}

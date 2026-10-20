using System;
using System.Linq;
using System.Reflection;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Data.Models;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogQuality.Tests.Unit;

[Trait("Category", "Unit")]
public class EntityMappingTests
{
    private static readonly DateTime CreatedDate = new(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ModifiedDate = new(2026, 10, 2, 9, 30, 0, DateTimeKind.Utc);

    // Audit fields and the key are owned by the platform — Patch must never overwrite them
    private static readonly string[] NotPatchedProperties =
    [
        nameof(Entity.Id),
        nameof(AuditableEntity.CreatedDate),
        nameof(AuditableEntity.CreatedBy),
        nameof(AuditableEntity.ModifiedDate),
        nameof(AuditableEntity.ModifiedBy),
    ];

    private static QualityData CreateQualityData()
    {
        return new QualityData
        {
            Id = "r1",
            CreatedBy = "creator",
            CreatedDate = CreatedDate,
            ModifiedBy = "modifier",
            ModifiedDate = ModifiedDate,
            EntityId = "p1",
            EntityType = "VirtoCommerce.CatalogModule.Core.Model.CatalogProduct",
            TargetId = "t1",
            ReadinessScore = 85,
            Suggestions = "### Quality score: 85 / 100",
            SuggestionsUrl = "catalog-quality/suggestions/p1/t1.md",
        };
    }

    private static QualityTarget CreateQualityTarget()
    {
        return new QualityTarget
        {
            Id = "t1",
            CreatedBy = "creator",
            CreatedDate = CreatedDate,
            ModifiedBy = "modifier",
            ModifiedDate = ModifiedDate,
            Name = "Catalog-Publication-China",
            Description = "Readiness for Tmall, JD",
            Prompt = "Evaluate images, properties and descriptions",
        };
    }

    // --- QualityDataEntity ---

    [Fact]
    public void QualityData_FromModel_CopiesAllStoredFields()
    {
        // Arrange
        var model = CreateQualityData();

        // Act
        var entity = new QualityDataEntity().FromModel(model, new PrimaryKeyResolvingMap());

        // Assert
        Assert.Equal("r1", entity.Id);
        Assert.Equal("creator", entity.CreatedBy);
        Assert.Equal(CreatedDate, entity.CreatedDate);
        Assert.Equal("modifier", entity.ModifiedBy);
        Assert.Equal(ModifiedDate, entity.ModifiedDate);
        Assert.Equal("p1", entity.EntityId);
        Assert.Equal("VirtoCommerce.CatalogModule.Core.Model.CatalogProduct", entity.EntityType);
        Assert.Equal("t1", entity.TargetId);
        Assert.Equal(85, entity.ReadinessScore);
        Assert.Equal("catalog-quality/suggestions/p1/t1.md", entity.SuggestionsUrl);
    }

    [Fact]
    public void QualityData_ToModel_CopiesAllStoredFieldsAndLeavesSuggestionsToTheService()
    {
        // Arrange
        var entity = new QualityDataEntity().FromModel(CreateQualityData(), new PrimaryKeyResolvingMap());

        // Act
        var model = entity.ToModel(new QualityData());

        // Assert
        Assert.Equal("r1", model.Id);
        Assert.Equal("creator", model.CreatedBy);
        Assert.Equal(CreatedDate, model.CreatedDate);
        Assert.Equal("modifier", model.ModifiedBy);
        Assert.Equal(ModifiedDate, model.ModifiedDate);
        Assert.Equal("p1", model.EntityId);
        Assert.Equal("VirtoCommerce.CatalogModule.Core.Model.CatalogProduct", model.EntityType);
        Assert.Equal("t1", model.TargetId);
        Assert.Equal(85, model.ReadinessScore);
        Assert.Equal("catalog-quality/suggestions/p1/t1.md", model.SuggestionsUrl);
        // The text lives in the suggestions file, not in the DB — QualityDataService loads it
        Assert.Null(model.Suggestions);
    }

    [Fact]
    public void QualityData_Patch_CopiesDataFieldsButKeepsKeyAndAudit()
    {
        // Arrange
        var source = new QualityDataEntity().FromModel(CreateQualityData(), new PrimaryKeyResolvingMap());
        var target = new QualityDataEntity
        {
            Id = "existing",
            CreatedBy = "original creator",
            CreatedDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EntityId = "old",
            EntityType = "old",
            TargetId = "old",
            ReadinessScore = 1,
            SuggestionsUrl = "old.md",
        };

        // Act
        source.Patch(target);

        // Assert
        Assert.Equal("existing", target.Id);
        Assert.Equal("original creator", target.CreatedBy);
        Assert.Equal(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), target.CreatedDate);
        Assert.Equal("p1", target.EntityId);
        Assert.Equal("VirtoCommerce.CatalogModule.Core.Model.CatalogProduct", target.EntityType);
        Assert.Equal("t1", target.TargetId);
        Assert.Equal(85, target.ReadinessScore);
        Assert.Equal("catalog-quality/suggestions/p1/t1.md", target.SuggestionsUrl);
    }

    [Fact]
    public void QualityData_FromModel_RegistersModelForPrimaryKeyResolving()
    {
        // Arrange
        var model = CreateQualityData();
        model.Id = null;
        var pkMap = new PrimaryKeyResolvingMap();
        var entity = new QualityDataEntity().FromModel(model, pkMap);

        // Act — the database generated the key on insert
        entity.Id = "generated";
        pkMap.ResolvePrimaryKeys();

        // Assert
        Assert.Equal("generated", model.Id);
    }

    [Fact]
    public void QualityData_NullArguments_Throw()
    {
        // Arrange
        var entity = new QualityDataEntity();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.FromModel(null, new PrimaryKeyResolvingMap()));
        Assert.Throws<ArgumentNullException>(() => entity.ToModel(null));
        Assert.Throws<ArgumentNullException>(() => entity.Patch(null));
    }

    [Fact]
    public void QualityData_EveryStoredModelProperty_SurvivesRoundTrip()
    {
        // Arrange — Suggestions is stored in a file, not in the DB
        var model = FillAllProperties(new QualityData(), nameof(QualityData.Suggestions));

        // Act
        var restored = new QualityDataEntity().FromModel(model, new PrimaryKeyResolvingMap()).ToModel(new QualityData());

        // Assert
        AssertPropertiesEqual(model, restored, nameof(QualityData.Suggestions));
    }

    [Fact]
    public void QualityData_Patch_CopiesEveryDataProperty()
    {
        // Arrange
        var source = FillAllProperties(new QualityDataEntity());
        var target = new QualityDataEntity();

        // Act
        source.Patch(target);

        // Assert
        AssertPropertiesEqual(source, target, NotPatchedProperties);
    }

    // --- QualityTargetEntity ---

    [Fact]
    public void QualityTarget_FromModelAndToModel_CopyAllFields()
    {
        // Arrange
        var model = CreateQualityTarget();

        // Act
        var restored = new QualityTargetEntity().FromModel(model, new PrimaryKeyResolvingMap()).ToModel(new QualityTarget());

        // Assert
        Assert.Equal("t1", restored.Id);
        Assert.Equal("creator", restored.CreatedBy);
        Assert.Equal(CreatedDate, restored.CreatedDate);
        Assert.Equal("modifier", restored.ModifiedBy);
        Assert.Equal(ModifiedDate, restored.ModifiedDate);
        Assert.Equal("Catalog-Publication-China", restored.Name);
        Assert.Equal("Readiness for Tmall, JD", restored.Description);
        Assert.Equal("Evaluate images, properties and descriptions", restored.Prompt);
    }

    [Fact]
    public void QualityTarget_Patch_CopiesDataFieldsButKeepsKeyAndAudit()
    {
        // Arrange
        var source = new QualityTargetEntity().FromModel(CreateQualityTarget(), new PrimaryKeyResolvingMap());
        var target = new QualityTargetEntity
        {
            Id = "existing",
            CreatedBy = "original creator",
            Name = "old",
            Description = "old",
            Prompt = "old",
        };

        // Act
        source.Patch(target);

        // Assert
        Assert.Equal("existing", target.Id);
        Assert.Equal("original creator", target.CreatedBy);
        Assert.Equal("Catalog-Publication-China", target.Name);
        Assert.Equal("Readiness for Tmall, JD", target.Description);
        Assert.Equal("Evaluate images, properties and descriptions", target.Prompt);
    }

    [Fact]
    public void QualityTarget_NullArguments_Throw()
    {
        // Arrange
        var entity = new QualityTargetEntity();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.FromModel(null, new PrimaryKeyResolvingMap()));
        Assert.Throws<ArgumentNullException>(() => entity.ToModel(null));
        Assert.Throws<ArgumentNullException>(() => entity.Patch(null));
    }

    [Fact]
    public void QualityTarget_EveryModelProperty_SurvivesRoundTrip()
    {
        // Arrange
        var model = FillAllProperties(new QualityTarget());

        // Act
        var restored = new QualityTargetEntity().FromModel(model, new PrimaryKeyResolvingMap()).ToModel(new QualityTarget());

        // Assert
        AssertPropertiesEqual(model, restored);
    }

    [Fact]
    public void QualityTarget_Patch_CopiesEveryDataProperty()
    {
        // Arrange
        var source = FillAllProperties(new QualityTargetEntity());
        var target = new QualityTargetEntity();

        // Act
        source.Patch(target);

        // Assert
        AssertPropertiesEqual(source, target, NotPatchedProperties);
    }

    // --- Reflection helpers: catch new fields forgotten in FromModel / ToModel / Patch ---

    private static PropertyInfo[] GetMappedProperties(Type type, string[] excluded)
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead && x.CanWrite && !excluded.Contains(x.Name))
            .Where(x => x.PropertyType == typeof(string) || x.PropertyType.IsValueType)
            .ToArray();
    }

    private static T FillAllProperties<T>(T instance, params string[] excluded)
    {
        foreach (var property in GetMappedProperties(typeof(T), excluded))
        {
            property.SetValue(instance, CreateValue(property));
        }

        return instance;
    }

    private static object CreateValue(PropertyInfo property)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (type == typeof(string))
        {
            return $"{property.Name}-value";
        }

        if (type == typeof(int))
        {
            return 42;
        }

        if (type == typeof(DateTime))
        {
            return ModifiedDate;
        }

        if (type == typeof(bool))
        {
            return true;
        }

        throw new NotSupportedException($"Add a test value for {property.Name} of type {type.Name}");
    }

    private static void AssertPropertiesEqual<T>(T expected, T actual, params string[] excluded)
    {
        foreach (var property in GetMappedProperties(typeof(T), excluded))
        {
            var expectedValue = property.GetValue(expected);
            var actualValue = property.GetValue(actual);

            Assert.True(Equals(expectedValue, actualValue),
                $"{typeof(T).Name}.{property.Name} was not mapped: expected '{expectedValue}', got '{actualValue}'");
        }
    }
}

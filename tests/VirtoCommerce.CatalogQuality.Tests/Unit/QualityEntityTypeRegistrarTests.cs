using System;
using System.Linq;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.CatalogQuality.Data.Services;
using Xunit;

namespace VirtoCommerce.CatalogQuality.Tests.Unit;

[Trait("Category", "Unit")]
public class QualityEntityTypeRegistrarTests
{
    private const string ProductType = "VirtoCommerce.CatalogModule.Core.Model.CatalogProduct";

    private class SellerProduct;

    [Fact]
    public void GetAll_NothingRegistered_ReturnsEmpty()
    {
        // Arrange
        var registrar = new QualityEntityTypeRegistrar();

        // Act
        var result = registrar.GetAll();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Register_ReturnsTypesInRegistrationOrder()
    {
        // Arrange
        var registrar = new QualityEntityTypeRegistrar();

        // Act
        registrar.Register(ProductType, "Catalog product");
        registrar.Register("Other.Type", "Other");

        // Assert
        Assert.Equal([ProductType, "Other.Type"], registrar.GetAll().Select(x => x.Id));
        Assert.Equal(["Catalog product", "Other"], registrar.GetAll().Select(x => x.Name));
    }

    [Fact]
    public void Register_Generic_UsesFullTypeName()
    {
        // Arrange
        var registrar = new QualityEntityTypeRegistrar();

        // Act
        registrar.Register<SellerProduct>("Seller product");

        // Assert
        var result = Assert.Single(registrar.GetAll());
        Assert.Equal(typeof(SellerProduct).FullName, result.Id);
        Assert.Equal("Seller product", result.Name);
    }

    [Fact]
    public void Register_SameIdIgnoringCase_ReplacesName()
    {
        // Arrange
        var registrar = new QualityEntityTypeRegistrar();
        registrar.Register(ProductType, "Catalog product");

        // Act
        registrar.Register(ProductType.ToUpperInvariant(), "Product");

        // Assert
        var result = Assert.Single(registrar.GetAll());
        Assert.Equal("Product", result.Name);
    }

    [Fact]
    public void Register_ThenChangeOriginal_KeepsRegisteredValues()
    {
        // Arrange
        var registrar = new QualityEntityTypeRegistrar();
        var entityType = new QualityEntityType { Id = ProductType, Name = "Catalog product" };

        // Act
        registrar.Register(entityType);
        entityType.Name = "Changed";
        registrar.GetAll()[0].Name = "Changed too";

        // Assert
        Assert.Equal("Catalog product", registrar.GetAll()[0].Name);
    }

    [Fact]
    public void Register_Null_Throws()
    {
        // Arrange
        var registrar = new QualityEntityTypeRegistrar();

        // Act
        var act = () => registrar.Register(null);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Theory]
    [InlineData(null, "Name")]
    [InlineData(" ", "Name")]
    [InlineData(ProductType, null)]
    [InlineData(ProductType, "")]
    public void Register_EmptyIdOrName_Throws(string id, string name)
    {
        // Arrange
        var registrar = new QualityEntityTypeRegistrar();

        // Act
        var act = () => registrar.Register(id, name);

        // Assert
        Assert.ThrowsAny<ArgumentException>(act);
        Assert.Empty(registrar.GetAll());
    }
}

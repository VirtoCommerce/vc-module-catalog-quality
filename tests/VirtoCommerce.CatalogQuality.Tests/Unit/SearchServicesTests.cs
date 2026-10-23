using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CatalogQuality.Core.Models.Search;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.CatalogQuality.Data.Models;
using VirtoCommerce.CatalogQuality.Data.Repositories;
using VirtoCommerce.CatalogQuality.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using Xunit;

namespace VirtoCommerce.CatalogQuality.Tests.Unit;

[Trait("Category", "Unit")]
public class SearchServicesTests
{
    private const string ProductType = "VirtoCommerce.CatalogModule.Core.Model.CatalogProduct";

    private readonly Mock<ICatalogQualityRepository> _repositoryMock = new();

    private TestQualityDataSearchService CreateDataSearchService(params QualityDataEntity[] entities)
    {
        _repositoryMock.Setup(x => x.QualityDatas).Returns(entities.AsQueryable());

        return new TestQualityDataSearchService(
            () => _repositoryMock.Object,
            Mock.Of<IPlatformMemoryCache>(),
            Mock.Of<IQualityDataCrudService>(),
            Options.Create(new CrudOptions()));
    }

    private TestQualityTargetSearchService CreateTargetSearchService(params QualityTargetEntity[] entities)
    {
        _repositoryMock.Setup(x => x.QualityTargets).Returns(entities.AsQueryable());

        return new TestQualityTargetSearchService(
            () => _repositoryMock.Object,
            Mock.Of<IPlatformMemoryCache>(),
            Mock.Of<IQualityTargetCrudService>(),
            Options.Create(new CrudOptions()));
    }

    private static QualityDataEntity Data(string id, string entityId, string entityType = ProductType)
    {
        return new QualityDataEntity { Id = id, EntityId = entityId, EntityType = entityType };
    }

    private static QualityTargetEntity Target(string id, string name, string entityType = ProductType)
    {
        return new QualityTargetEntity { Id = id, Name = name, EntityType = entityType };
    }

    // --- QualityDataSearchService ---

    [Fact]
    public void DataSearch_NoFilters_ReturnsAllRecords()
    {
        // Arrange
        var service = CreateDataSearchService(Data("r1", "p1"), Data("r2", "p2"));

        // Act
        var result = service.Query(new SearchQualityDataCriteria());

        // Assert
        Assert.Equal(["r1", "r2"], result);
    }

    [Fact]
    public void DataSearch_Keyword_FiltersByEntityIdPart()
    {
        // Arrange
        var service = CreateDataSearchService(Data("r1", "product-001"), Data("r2", "product-002"), Data("r3", "other"));

        // Act
        var result = service.Query(new SearchQualityDataCriteria { Keyword = "product" });

        // Assert
        Assert.Equal(["r1", "r2"], result);
    }

    [Fact]
    public void DataSearch_EntityId_FiltersByExactEntityId()
    {
        // Arrange
        var service = CreateDataSearchService(Data("r1", "p1"), Data("r2", "p10"));

        // Act
        var result = service.Query(new SearchQualityDataCriteria { EntityId = "p1" });

        // Assert
        Assert.Equal(["r1"], result);
    }

    [Fact]
    public void DataSearch_EntityType_FiltersByEntityType()
    {
        // Arrange
        var service = CreateDataSearchService(Data("r1", "p1"), Data("r2", "p1", "Other.EntityType"));

        // Act
        var result = service.Query(new SearchQualityDataCriteria { EntityType = ProductType });

        // Assert
        Assert.Equal(["r1"], result);
    }

    [Fact]
    public void DataSearch_SeveralFilters_AreCombined()
    {
        // Arrange
        var service = CreateDataSearchService(Data("r1", "p1"), Data("r2", "p1", "Other.EntityType"), Data("r3", "p2"));

        // Act
        var result = service.Query(new SearchQualityDataCriteria { EntityId = "p1", EntityType = ProductType });

        // Assert
        Assert.Equal(["r1"], result);
    }

    [Fact]
    public void DataSearch_NoSort_SortsByCreatedDateDescending()
    {
        // Arrange
        var service = CreateDataSearchService();

        // Act
        var sortInfos = service.Sort(new SearchQualityDataCriteria());

        // Assert
        var sortInfo = Assert.Single(sortInfos);
        Assert.Equal(nameof(QualityDataEntity.CreatedDate), sortInfo.SortColumn);
        Assert.Equal(SortDirection.Descending, sortInfo.SortDirection);
    }

    [Fact]
    public void DataSearch_GivenSort_IsKept()
    {
        // Arrange
        var service = CreateDataSearchService();

        // Act
        var sortInfos = service.Sort(new SearchQualityDataCriteria { Sort = "readinessScore:asc" });

        // Assert
        var sortInfo = Assert.Single(sortInfos);
        Assert.Equal("readinessScore", sortInfo.SortColumn);
        Assert.Equal(SortDirection.Ascending, sortInfo.SortDirection);
    }

    // --- QualityTargetSearchService ---

    [Fact]
    public void TargetSearch_NoFilters_ReturnsAllTargets()
    {
        // Arrange
        var service = CreateTargetSearchService(Target("t1", "Catalog Publishing"), Target("t2", "Amazon DE", "Other.EntityType"));

        // Act
        var result = service.Query(new SearchQualityTargetCriteria());

        // Assert
        Assert.Equal(["t1", "t2"], result);
    }

    [Fact]
    public void TargetSearch_ObjectIds_FiltersByIds()
    {
        // Arrange
        var service = CreateTargetSearchService(Target("t1", "A"), Target("t2", "B"), Target("t3", "C"));

        // Act
        var result = service.Query(new SearchQualityTargetCriteria { ObjectIds = ["t1", "t3"] });

        // Assert
        Assert.Equal(["t1", "t3"], result);
    }

    [Fact]
    public void TargetSearch_Keyword_FiltersByNamePart()
    {
        // Arrange
        var service = CreateTargetSearchService(Target("t1", "Catalog-Publication-China"), Target("t2", "Amazon-DE"));

        // Act
        var result = service.Query(new SearchQualityTargetCriteria { Keyword = "China" });

        // Assert
        Assert.Equal(["t1"], result);
    }

    [Fact]
    public void TargetSearch_ObjectIdsAndKeyword_AreCombined()
    {
        // Arrange
        var service = CreateTargetSearchService(Target("t1", "Publication China"), Target("t2", "Publication DE"));

        // Act
        var result = service.Query(new SearchQualityTargetCriteria { ObjectIds = ["t1", "t2"], Keyword = "DE" });

        // Assert
        Assert.Equal(["t2"], result);
    }

    [Fact]
    public void TargetSearch_EntityType_FiltersByEntityType()
    {
        // Arrange
        var service = CreateTargetSearchService(Target("t1", "Publication China"), Target("t2", "Publication DE", "Other.EntityType"));

        // Act
        var result = service.Query(new SearchQualityTargetCriteria { EntityType = ProductType });

        // Assert
        Assert.Equal(["t1"], result);
    }

    [Fact]
    public void TargetSearch_EntityTypeAndKeyword_AreCombined()
    {
        // Arrange
        var service = CreateTargetSearchService(
            Target("t1", "Publication China"),
            Target("t2", "Publication DE"),
            Target("t3", "Publication China", "Other.EntityType"));

        // Act
        var result = service.Query(new SearchQualityTargetCriteria { EntityType = ProductType, Keyword = "China" });

        // Assert
        Assert.Equal(["t1"], result);
    }

    [Fact]
    public void TargetSearch_NoSort_SortsByNameAscending()
    {
        // Arrange
        var service = CreateTargetSearchService();

        // Act
        var sortInfos = service.Sort(new SearchQualityTargetCriteria());

        // Assert
        var sortInfo = Assert.Single(sortInfos);
        Assert.Equal(nameof(QualityTargetEntity.Name), sortInfo.SortColumn);
        Assert.Equal(SortDirection.Ascending, sortInfo.SortDirection);
    }

    /// <summary>
    /// Exposes the protected query and sort building of the search service.
    /// </summary>
    private sealed class TestQualityDataSearchService(
        System.Func<ICatalogQualityRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IQualityDataCrudService crudService,
        IOptions<CrudOptions> crudOptions)
        : QualityDataSearchService(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
        private readonly System.Func<ICatalogQualityRepository> _repositoryFactory = repositoryFactory;

        public IList<string> Query(SearchQualityDataCriteria criteria)
        {
            return BuildQuery(_repositoryFactory(), criteria).Select(x => x.Id).OrderBy(x => x).ToList();
        }

        public IList<SortInfo> Sort(SearchQualityDataCriteria criteria)
        {
            return BuildSortExpression(criteria);
        }
    }

    /// <summary>
    /// Exposes the protected query and sort building of the search service.
    /// </summary>
    private sealed class TestQualityTargetSearchService(
        System.Func<ICatalogQualityRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IQualityTargetCrudService crudService,
        IOptions<CrudOptions> crudOptions)
        : QualityTargetSearchService(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
        private readonly System.Func<ICatalogQualityRepository> _repositoryFactory = repositoryFactory;

        public IList<string> Query(SearchQualityTargetCriteria criteria)
        {
            return BuildQuery(_repositoryFactory(), criteria).Select(x => x.Id).OrderBy(x => x).ToList();
        }

        public IList<SortInfo> Sort(SearchQualityTargetCriteria criteria)
        {
            return BuildSortExpression(criteria);
        }
    }
}

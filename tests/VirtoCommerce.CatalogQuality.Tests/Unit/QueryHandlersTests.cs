using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Models.Search;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.CatalogQuality.Data.Queries;
using Xunit;

namespace VirtoCommerce.CatalogQuality.Tests.Unit;

[Trait("Category", "Unit")]
public class QueryHandlersTests
{
    private const string ProductType = "VirtoCommerce.CatalogModule.Core.Model.CatalogProduct";

    private readonly Mock<IQualityDataService> _qualityDataServiceMock = new();
    private readonly Mock<IQualityTargetSearchService> _qualityTargetSearchServiceMock = new();
    private readonly Mock<IQualityTargetCrudService> _qualityTargetCrudServiceMock = new();

    // --- GetQualityDataQueryHandler ---

    [Fact]
    public async Task GetQualityData_ReturnsRecordOfEntityAndTarget()
    {
        // Arrange
        var expected = new QualityData { Id = "r1" };
        _qualityDataServiceMock
            .Setup(x => x.GetQualityDataByEntityAsync("p1", ProductType, "t1"))
            .ReturnsAsync(expected);
        var handler = new GetQualityDataQueryHandler(_qualityDataServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetQualityDataQuery { EntityId = "p1", EntityType = ProductType, TargetId = "t1" }, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetQualityData_WithoutTarget_PassesNullTarget()
    {
        // Arrange
        var handler = new GetQualityDataQueryHandler(_qualityDataServiceMock.Object);

        // Act
        await handler.Handle(new GetQualityDataQuery { EntityId = "p1", EntityType = ProductType }, CancellationToken.None);

        // Assert
        _qualityDataServiceMock.Verify(x => x.GetQualityDataByEntityAsync("p1", ProductType, null), Times.Once);
    }

    [Fact]
    public async Task GetQualityData_NullRequest_Throws()
    {
        // Arrange
        var handler = new GetQualityDataQueryHandler(_qualityDataServiceMock.Object);

        // Act
        var act = () => handler.Handle(null, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task GetQualityData_NullEntityId_Throws()
    {
        // Arrange
        var handler = new GetQualityDataQueryHandler(_qualityDataServiceMock.Object);

        // Act
        var act = () => handler.Handle(new GetQualityDataQuery { EntityType = ProductType }, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task GetQualityData_NullEntityType_Throws()
    {
        // Arrange
        var handler = new GetQualityDataQueryHandler(_qualityDataServiceMock.Object);

        // Act
        var act = () => handler.Handle(new GetQualityDataQuery { EntityId = "p1" }, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    // --- SearchQualityDataQueryHandler ---

    [Fact]
    public async Task SearchQualityData_ReturnsServiceResult()
    {
        // Arrange
        var query = new SearchQualityDataQuery { Keyword = "p1" };
        var expected = new SearchQualityDataResult();
        _qualityDataServiceMock.Setup(x => x.SearchAsync(query)).ReturnsAsync(expected);
        var handler = new SearchQualityDataQueryHandler(_qualityDataServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
    }

    [Fact]
    public async Task SearchQualityData_NullRequest_Throws()
    {
        // Arrange
        var handler = new SearchQualityDataQueryHandler(_qualityDataServiceMock.Object);

        // Act
        var act = () => handler.Handle(null, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    // --- GetQualityTargetQueryHandler ---

    [Fact]
    public async Task GetQualityTarget_ReturnsTargetById()
    {
        // Arrange
        var expected = new QualityTarget { Id = "t1" };
        _qualityTargetCrudServiceMock
            .Setup(x => x.GetAsync(It.Is<IList<string>>(ids => ids.Single() == "t1"), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([expected]);
        var handler = new GetQualityTargetQueryHandler(_qualityTargetCrudServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetQualityTargetQuery { Id = "t1" }, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetQualityTarget_UnknownId_ReturnsNull()
    {
        // Arrange
        _qualityTargetCrudServiceMock
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([]);
        var handler = new GetQualityTargetQueryHandler(_qualityTargetCrudServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetQualityTargetQuery { Id = "missing" }, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetQualityTarget_NullRequest_Throws()
    {
        // Arrange
        var handler = new GetQualityTargetQueryHandler(_qualityTargetCrudServiceMock.Object);

        // Act
        var act = () => handler.Handle(null, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task GetQualityTarget_NullId_Throws()
    {
        // Arrange
        var handler = new GetQualityTargetQueryHandler(_qualityTargetCrudServiceMock.Object);

        // Act
        var act = () => handler.Handle(new GetQualityTargetQuery(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    // --- SearchQualityTargetQueryHandler ---

    [Fact]
    public async Task SearchQualityTarget_ReturnsServiceResult()
    {
        // Arrange
        var query = new SearchQualityTargetQuery { Keyword = "China" };
        var expected = new SearchQualityTargetResult();
        _qualityTargetSearchServiceMock.Setup(x => x.SearchAsync(query, It.IsAny<bool>())).ReturnsAsync(expected);
        var handler = new SearchQualityTargetQueryHandler(_qualityTargetSearchServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
    }

    [Fact]
    public async Task SearchQualityTarget_NullRequest_Throws()
    {
        // Arrange
        var handler = new SearchQualityTargetQueryHandler(_qualityTargetSearchServiceMock.Object);

        // Act
        var act = () => handler.Handle(null, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }
}

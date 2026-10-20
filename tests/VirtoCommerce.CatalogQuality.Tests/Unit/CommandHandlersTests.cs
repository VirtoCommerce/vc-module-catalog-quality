using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogQuality.Core.Events;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.CatalogQuality.Data.Commands;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogQuality.Tests.Unit;

[Trait("Category", "Unit")]
public class CommandHandlersTests
{
    private readonly Mock<IQualityDataService> _qualityDataServiceMock = new();
    private readonly Mock<IQualityTargetCrudService> _qualityTargetCrudServiceMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();

    // --- UpdateQualityDataCommandHandler ---

    [Fact]
    public async Task UpdateQualityData_SavesTheRecord()
    {
        // Arrange
        var handler = new UpdateQualityDataCommandHandler(_qualityDataServiceMock.Object);
        var qualityData = new QualityData { EntityId = "p1", TargetId = "t1" };

        // Act
        await handler.Handle(new UpdateQualityDataCommand { QualityData = qualityData }, CancellationToken.None);

        // Assert
        _qualityDataServiceMock.Verify(x => x.SaveChangesAsync(It.Is<IList<QualityData>>(models => models.Single() == qualityData)), Times.Once);
    }

    [Fact]
    public async Task UpdateQualityData_NullRequest_Throws()
    {
        // Arrange
        var handler = new UpdateQualityDataCommandHandler(_qualityDataServiceMock.Object);

        // Act
        var act = () => handler.Handle(null, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task UpdateQualityData_NullQualityData_Throws()
    {
        // Arrange
        var handler = new UpdateQualityDataCommandHandler(_qualityDataServiceMock.Object);

        // Act
        var act = () => handler.Handle(new UpdateQualityDataCommand(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        _qualityDataServiceMock.Verify(x => x.SaveChangesAsync(It.IsAny<IList<QualityData>>()), Times.Never);
    }

    // --- BulkUpdateQualityDataCommandHandler ---

    [Fact]
    public async Task BulkUpdateQualityData_SavesAllRecordsInOneCall()
    {
        // Arrange
        var handler = new BulkUpdateQualityDataCommandHandler(_qualityDataServiceMock.Object);
        var first = new QualityData { EntityId = "p1", TargetId = "t1" };
        var second = new QualityData { EntityId = "p1", TargetId = "t2" };

        // Act
        await handler.Handle(new BulkUpdateQualityDataCommand { QualityDatas = [first, second] }, CancellationToken.None);

        // Assert
        _qualityDataServiceMock.Verify(x => x.SaveChangesAsync(It.Is<IList<QualityData>>(models => models.SequenceEqual(new[] { first, second }))), Times.Once);
    }

    [Fact]
    public async Task BulkUpdateQualityData_SkipsNullItems()
    {
        // Arrange
        var handler = new BulkUpdateQualityDataCommandHandler(_qualityDataServiceMock.Object);
        var qualityData = new QualityData { EntityId = "p1", TargetId = "t1" };

        // Act
        await handler.Handle(new BulkUpdateQualityDataCommand { QualityDatas = [null, qualityData, null] }, CancellationToken.None);

        // Assert
        _qualityDataServiceMock.Verify(x => x.SaveChangesAsync(It.Is<IList<QualityData>>(models => models.Single() == qualityData)), Times.Once);
    }

    [Theory]
    [MemberData(nameof(EmptyQualityDataLists))]
    public async Task BulkUpdateQualityData_NothingToSave_DoesNotCallService(IList<QualityData> qualityDatas)
    {
        // Arrange
        var handler = new BulkUpdateQualityDataCommandHandler(_qualityDataServiceMock.Object);

        // Act
        await handler.Handle(new BulkUpdateQualityDataCommand { QualityDatas = qualityDatas }, CancellationToken.None);

        // Assert
        _qualityDataServiceMock.Verify(x => x.SaveChangesAsync(It.IsAny<IList<QualityData>>()), Times.Never);
    }

    public static TheoryData<IList<QualityData>> EmptyQualityDataLists => new()
    {
        null,
        new List<QualityData>(),
        new List<QualityData> { null, null },
    };

    [Fact]
    public async Task BulkUpdateQualityData_NullRequest_Throws()
    {
        // Arrange
        var handler = new BulkUpdateQualityDataCommandHandler(_qualityDataServiceMock.Object);

        // Act
        var act = () => handler.Handle(null, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    // --- UpdateQualityTargetCommandHandler ---

    [Fact]
    public async Task UpdateQualityTarget_SavesTheTarget()
    {
        // Arrange
        var handler = CreateUpdateQualityTargetHandler();
        var target = new QualityTarget { Name = "Catalog Publishing" };

        // Act
        await handler.Handle(new UpdateQualityTargetCommand { QualityTarget = target }, CancellationToken.None);

        // Assert
        _qualityTargetCrudServiceMock.Verify(x => x.SaveChangesAsync(It.Is<IList<QualityTarget>>(models => models.Single() == target)), Times.Once);
    }

    [Fact]
    public async Task UpdateQualityTarget_NewTarget_DoesNotPublishChangedEvent()
    {
        // Arrange
        var handler = CreateUpdateQualityTargetHandler();
        var target = new QualityTarget { Name = "Catalog Publishing" };

        // Act
        await handler.Handle(new UpdateQualityTargetCommand { QualityTarget = target }, CancellationToken.None);

        // Assert
        _eventPublisherMock.Verify(x => x.Publish(It.IsAny<TargetChangedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateQualityTarget_ExistingTarget_PublishesChangedEventAfterSave()
    {
        // Arrange
        var handler = CreateUpdateQualityTargetHandler();
        var target = new QualityTarget { Id = "t1", Name = "Catalog Publishing" };
        var calls = new List<string>();
        TargetChangedEvent publishedEvent = null;

        _qualityTargetCrudServiceMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<QualityTarget>>()))
            .Callback(() => calls.Add("save"))
            .Returns(Task.CompletedTask);
        _eventPublisherMock
            .Setup(x => x.Publish(It.IsAny<TargetChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TargetChangedEvent, CancellationToken>((e, _) =>
            {
                calls.Add("publish");
                publishedEvent = e;
            })
            .Returns(Task.CompletedTask);

        // Act
        await handler.Handle(new UpdateQualityTargetCommand { QualityTarget = target }, CancellationToken.None);

        // Assert
        Assert.Equal(["save", "publish"], calls);
        var entry = Assert.Single(publishedEvent.ChangedEntries);
        Assert.Same(target, entry.NewEntry);
        Assert.Equal(EntryState.Modified, entry.EntryState);
    }

    [Fact]
    public async Task UpdateQualityTarget_NullRequest_Throws()
    {
        // Arrange
        var handler = CreateUpdateQualityTargetHandler();

        // Act
        var act = () => handler.Handle(null, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task UpdateQualityTarget_NullTarget_Throws()
    {
        // Arrange
        var handler = CreateUpdateQualityTargetHandler();

        // Act
        var act = () => handler.Handle(new UpdateQualityTargetCommand(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        _qualityTargetCrudServiceMock.Verify(x => x.SaveChangesAsync(It.IsAny<IList<QualityTarget>>()), Times.Never);
        _eventPublisherMock.Verify(x => x.Publish(It.IsAny<TargetChangedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private UpdateQualityTargetCommandHandler CreateUpdateQualityTargetHandler()
    {
        return new UpdateQualityTargetCommandHandler(_qualityTargetCrudServiceMock.Object, _eventPublisherMock.Object);
    }

    // --- DeleteQualityTargetCommandHandler ---

    [Fact]
    public async Task DeleteQualityTarget_DeletesTargetsById()
    {
        // Arrange
        var handler = new DeleteQualityTargetCommandHandler(_qualityTargetCrudServiceMock.Object);

        // Act
        await handler.Handle(new DeleteQualityTargetCommand { Ids = ["t1", "t2"] }, CancellationToken.None);

        // Assert
        _qualityTargetCrudServiceMock.Verify(x => x.DeleteAsync(It.Is<IList<string>>(ids => ids.SequenceEqual(new[] { "t1", "t2" })), It.IsAny<bool>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(EmptyIdLists))]
    public async Task DeleteQualityTarget_NoIds_DoesNotCallService(IList<string> ids)
    {
        // Arrange
        var handler = new DeleteQualityTargetCommandHandler(_qualityTargetCrudServiceMock.Object);

        // Act
        await handler.Handle(new DeleteQualityTargetCommand { Ids = ids }, CancellationToken.None);

        // Assert
        _qualityTargetCrudServiceMock.Verify(x => x.DeleteAsync(It.IsAny<IList<string>>(), It.IsAny<bool>()), Times.Never);
    }

    public static TheoryData<IList<string>> EmptyIdLists => new()
    {
        null,
        new List<string>(),
    };

    [Fact]
    public async Task DeleteQualityTarget_NullRequest_Throws()
    {
        // Arrange
        var handler = new DeleteQualityTargetCommandHandler(_qualityTargetCrudServiceMock.Object);

        // Act
        var act = () => handler.Handle(null, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }
}

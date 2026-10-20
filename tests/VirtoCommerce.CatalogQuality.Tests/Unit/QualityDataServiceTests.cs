using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Models.Search;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.CatalogQuality.Data.Models;
using VirtoCommerce.CatalogQuality.Data.Repositories;
using VirtoCommerce.CatalogQuality.Data.Services;
using Xunit;

namespace VirtoCommerce.CatalogQuality.Tests.Unit;

[Trait("Category", "Unit")]
public class QualityDataServiceTests
{
    private const string ProductType = "VirtoCommerce.CatalogModule.Core.Model.CatalogProduct";

    private readonly Mock<IQualityDataCrudService> _crudServiceMock = new();
    private readonly Mock<IQualityDataSearchService> _searchServiceMock = new();
    private readonly Mock<IBlobStorageProvider> _blobStorageMock = new();
    private readonly Mock<ICatalogQualityRepository> _repositoryMock = new();

    // In-memory blob storage: url -> content
    private readonly Dictionary<string, string> _blobs = new();

    // What the service passed to the CRUD service on save
    private IList<QualityData> _savedModels;

    // Existing records in the "database"
    private readonly List<QualityDataEntity> _existingEntities = [];

    public QualityDataServiceTests()
    {
        _crudServiceMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<QualityData>>()))
            .Callback<IList<QualityData>>(models => _savedModels = models)
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(x => x.GetQualityDatasByEntitiesAsync(It.IsAny<IList<string>>(), It.IsAny<IList<string>>()))
            .ReturnsAsync((IList<string> entityIds, IList<string> entityTypes) => _existingEntities
                .Where(x => entityIds.Contains(x.EntityId, StringComparer.OrdinalIgnoreCase) &&
                            entityTypes.Contains(x.EntityType, StringComparer.OrdinalIgnoreCase))
                .ToList());

        _blobStorageMock
            .Setup(x => x.OpenWriteAsync(It.IsAny<string>()))
            .ReturnsAsync((string url) => new CapturingStream(bytes => _blobs[url] = Encoding.UTF8.GetString(bytes)));

        _blobStorageMock
            .Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
            .ReturnsAsync((string url) => url != null && _blobs.ContainsKey(url) ? new BlobInfo() : null);

        _blobStorageMock
            .Setup(x => x.OpenReadAsync(It.IsAny<string>()))
            .ReturnsAsync((string url) => (Stream)new MemoryStream(Encoding.UTF8.GetBytes(_blobs[url])));
    }

    private QualityDataService CreateService()
    {
        return new QualityDataService(
            _crudServiceMock.Object,
            _searchServiceMock.Object,
            _blobStorageMock.Object,
            () => _repositoryMock.Object);
    }

    private static QualityData Evaluation(string productId, string targetId, int score = 50, string suggestions = null, string id = null)
    {
        return new QualityData
        {
            Id = id,
            EntityId = productId,
            EntityType = ProductType,
            TargetId = targetId,
            ReadinessScore = score,
            Suggestions = suggestions,
        };
    }

    private void AddExisting(string id, string productId, string targetId, string entityType = ProductType)
    {
        _existingEntities.Add(new QualityDataEntity
        {
            Id = id,
            EntityId = productId,
            EntityType = entityType,
            TargetId = targetId,
        });
    }

    // --- One record per entity and target ---

    [Fact]
    public async Task SaveChanges_NewProductWithSeveralTargets_SavesNewRecordForEachTarget()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", "t1"), Evaluation("p1", "t2"), Evaluation("p1", "t3")]);

        // Assert
        Assert.Equal(3, _savedModels.Count);
        Assert.All(_savedModels, x => Assert.Null(x.Id));
        Assert.Equal(["t1", "t2", "t3"], _savedModels.Select(x => x.TargetId).OrderBy(x => x));
    }

    [Fact]
    public async Task SaveChanges_Rescoring_ReusesIdsOfExistingRecords()
    {
        // Arrange
        AddExisting("r1", "p1", "t1");
        AddExisting("r2", "p1", "t2");
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", "t1", 70), Evaluation("p1", "t2", 80)]);

        // Assert
        Assert.Equal("r1", _savedModels.Single(x => x.TargetId == "t1").Id);
        Assert.Equal("r2", _savedModels.Single(x => x.TargetId == "t2").Id);
    }

    [Fact]
    public async Task SaveChanges_ExistingAndNewTargets_UpdatesExistingAndCreatesNew()
    {
        // Arrange
        AddExisting("r1", "p1", "t1");
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", "t1"), Evaluation("p1", "t2")]);

        // Assert
        Assert.Equal("r1", _savedModels.Single(x => x.TargetId == "t1").Id);
        Assert.Null(_savedModels.Single(x => x.TargetId == "t2").Id);
    }

    [Fact]
    public async Task SaveChanges_SeveralProducts_MatchesEachProductSeparately()
    {
        // Arrange
        AddExisting("r1", "p1", "t1");
        AddExisting("r2", "p2", "t1");
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", "t1"), Evaluation("p2", "t1"), Evaluation("p3", "t1")]);

        // Assert
        Assert.Equal("r1", _savedModels.Single(x => x.EntityId == "p1").Id);
        Assert.Equal("r2", _savedModels.Single(x => x.EntityId == "p2").Id);
        Assert.Null(_savedModels.Single(x => x.EntityId == "p3").Id);
    }

    [Fact]
    public async Task SaveChanges_SameProductAndTargetTwiceInRequest_KeepsOnlyTheLast()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", "t1", 10), Evaluation("p1", "t1", 90)]);

        // Assert
        var saved = Assert.Single(_savedModels);
        Assert.Equal(90, saved.ReadinessScore);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public async Task SaveChanges_EmptyAndNullTargetId_AreTheSameRecord(string existingTargetId, string newTargetId)
    {
        // Arrange
        AddExisting("r1", "p1", existingTargetId);
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", newTargetId)]);

        // Assert
        Assert.Equal("r1", Assert.Single(_savedModels).Id);
    }

    [Fact]
    public async Task SaveChanges_RecordWithoutTarget_IsNotMatchedToRecordWithTarget()
    {
        // Arrange
        AddExisting("r1", "p1", "t1");
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", null)]);

        // Assert
        Assert.Null(Assert.Single(_savedModels).Id);
    }

    [Fact]
    public async Task SaveChanges_MatchingIgnoresCase()
    {
        // Arrange
        AddExisting("r1", "P1", "T1");
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", "t1")]);

        // Assert
        Assert.Equal("r1", Assert.Single(_savedModels).Id);
    }

    [Fact]
    public async Task SaveChanges_SameEntityIdOfOtherEntityType_IsNotMatched()
    {
        // Arrange
        AddExisting("r1", "p1", "t1", entityType: "Other.EntityType");
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([
            Evaluation("p1", "t1"),
            new QualityData { EntityId = "p1", EntityType = "Other.EntityType", TargetId = "t2" },
        ]);

        // Assert
        Assert.Null(_savedModels.Single(x => x.EntityType == ProductType).Id);
    }

    [Fact]
    public async Task SaveChanges_ItemsWithIds_AreSavedAsIsWithoutLookup()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", "t1", id: "given")]);

        // Assert
        Assert.Equal("given", Assert.Single(_savedModels).Id);
        _repositoryMock.Verify(x => x.GetQualityDatasByEntitiesAsync(It.IsAny<IList<string>>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Fact]
    public async Task SaveChanges_ManyItems_LooksUpExistingRecordsInOneQuery()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", "t1"), Evaluation("p1", "t2"), Evaluation("p2", "t1")]);

        // Assert
        _repositoryMock.Verify(x => x.GetQualityDatasByEntitiesAsync(It.IsAny<IList<string>>(), It.IsAny<IList<string>>()), Times.Once);
    }

    // --- Suggestions files ---

    [Fact]
    public async Task SaveChanges_WritesSuggestionsToOneFilePerProductAndTarget()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", "t1", suggestions: "one"), Evaluation("p1", "t2", suggestions: "two")]);

        // Assert
        var first = _savedModels.Single(x => x.TargetId == "t1");
        var second = _savedModels.Single(x => x.TargetId == "t2");
        Assert.Equal($"catalog-quality/suggestions/{ProductType}/p1/t1.md", first.SuggestionsUrl);
        Assert.Equal($"catalog-quality/suggestions/{ProductType}/p1/t2.md", second.SuggestionsUrl);
        Assert.Equal("one", _blobs[first.SuggestionsUrl]);
        Assert.Equal("two", _blobs[second.SuggestionsUrl]);
    }

    [Fact]
    public async Task SaveChanges_WithoutTarget_WritesSuggestionsToLegacyPath()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", null, suggestions: "text")]);

        // Assert
        Assert.Equal($"catalog-quality/suggestions/{ProductType}/p1.md", Assert.Single(_savedModels).SuggestionsUrl);
    }

    [Fact]
    public async Task SaveChanges_NullSuggestions_DoesNotWriteFile()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SaveChangesAsync([Evaluation("p1", "t1", suggestions: null)]);

        // Assert
        Assert.Null(Assert.Single(_savedModels).SuggestionsUrl);
        _blobStorageMock.Verify(x => x.OpenWriteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetQualityDataByEntity_ReturnsRecordWithSuggestionsFromFile()
    {
        // Arrange
        var url = $"catalog-quality/suggestions/{ProductType}/p1/t1.md";
        _blobs[url] = "### Quality score: 80 / 100";
        _repositoryMock
            .Setup(x => x.GetQualityDataByEntityAsync("p1", ProductType, "t1"))
            .ReturnsAsync(new QualityDataEntity { Id = "r1", EntityId = "p1", EntityType = ProductType, TargetId = "t1", ReadinessScore = 80, SuggestionsUrl = url });
        var service = CreateService();

        // Act
        var result = await service.GetQualityDataByEntityAsync("p1", ProductType, "t1");

        // Assert
        Assert.Equal("r1", result.Id);
        Assert.Equal(80, result.ReadinessScore);
        Assert.Equal("### Quality score: 80 / 100", result.Suggestions);
    }

    [Fact]
    public async Task GetQualityDataByEntity_RecordWithoutSuggestionsFile_DoesNotTouchBlobStorage()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetQualityDataByEntityAsync("p1", ProductType, "t1"))
            .ReturnsAsync(new QualityDataEntity { Id = "r1", EntityId = "p1", EntityType = ProductType, TargetId = "t1", SuggestionsUrl = null });
        var service = CreateService();

        // Act
        var result = await service.GetQualityDataByEntityAsync("p1", ProductType, "t1");

        // Assert
        Assert.Equal("r1", result.Id);
        Assert.Null(result.Suggestions);
        _blobStorageMock.Verify(x => x.GetBlobInfoAsync(It.IsAny<string>()), Times.Never);
        _blobStorageMock.Verify(x => x.OpenReadAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetQualityDataByEntity_SuggestionsFileMissing_ReturnsRecordWithoutSuggestions()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetQualityDataByEntityAsync("p1", ProductType, "t1"))
            .ReturnsAsync(new QualityDataEntity { Id = "r1", EntityId = "p1", EntityType = ProductType, TargetId = "t1", SuggestionsUrl = "deleted.md" });
        var service = CreateService();

        // Act
        var result = await service.GetQualityDataByEntityAsync("p1", ProductType, "t1");

        // Assert
        Assert.Equal("r1", result.Id);
        Assert.Null(result.Suggestions);
        _blobStorageMock.Verify(x => x.OpenReadAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetQualityDataByEntity_NotFound_ReturnsNull()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.GetQualityDataByEntityAsync("p1", ProductType, "t1");

        // Assert
        Assert.Null(result);
    }

    // --- Search ---

    [Fact]
    public async Task Search_LoadsSuggestionsOfEachResultFromItsFile()
    {
        // Arrange
        _blobs["a.md"] = "suggestions A";
        _blobs["b.md"] = "suggestions B";
        var criteria = new SearchQualityDataCriteria();
        _searchServiceMock
            .Setup(x => x.SearchAsync(criteria, It.IsAny<bool>()))
            .ReturnsAsync(new SearchQualityDataResult
            {
                TotalCount = 2,
                Results = [new QualityData { Id = "r1", SuggestionsUrl = "a.md" }, new QualityData { Id = "r2", SuggestionsUrl = "b.md" }],
            });
        var service = CreateService();

        // Act
        var result = await service.SearchAsync(criteria);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal("suggestions A", result.Results.Single(x => x.Id == "r1").Suggestions);
        Assert.Equal("suggestions B", result.Results.Single(x => x.Id == "r2").Suggestions);
    }

    [Fact]
    public async Task Search_ResultsWithoutSuggestionsFile_AreReturnedWithoutReadingStorage()
    {
        // Arrange
        var criteria = new SearchQualityDataCriteria();
        _searchServiceMock
            .Setup(x => x.SearchAsync(criteria, It.IsAny<bool>()))
            .ReturnsAsync(new SearchQualityDataResult
            {
                TotalCount = 2,
                Results = [new QualityData { Id = "r1", SuggestionsUrl = null }, new QualityData { Id = "r2", SuggestionsUrl = "" }],
            });
        var service = CreateService();

        // Act
        var result = await service.SearchAsync(criteria);

        // Assert
        Assert.All(result.Results, x => Assert.Null(x.Suggestions));
        _blobStorageMock.Verify(x => x.GetBlobInfoAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Search_SuggestionsFileMissing_ReturnsResultWithoutSuggestions()
    {
        // Arrange
        var criteria = new SearchQualityDataCriteria();
        _searchServiceMock
            .Setup(x => x.SearchAsync(criteria, It.IsAny<bool>()))
            .ReturnsAsync(new SearchQualityDataResult
            {
                TotalCount = 1,
                Results = [new QualityData { Id = "r1", SuggestionsUrl = "deleted.md" }],
            });
        var service = CreateService();

        // Act
        var result = await service.SearchAsync(criteria);

        // Assert
        Assert.Null(Assert.Single(result.Results).Suggestions);
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_RemovesRecordsAndTheirSuggestionsFiles()
    {
        // Arrange
        _crudServiceMock
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([new QualityData { Id = "r1", SuggestionsUrl = "a.md" }, new QualityData { Id = "r2" }]);
        var service = CreateService();

        // Act
        await service.DeleteAsync(["r1", "r2"]);

        // Assert
        _crudServiceMock.Verify(x => x.DeleteAsync(It.Is<IList<string>>(ids => ids.SequenceEqual(new[] { "r1", "r2" })), It.IsAny<bool>()), Times.Once);
        _blobStorageMock.Verify(x => x.RemoveAsync(It.Is<string[]>(urls => urls.SequenceEqual(new[] { "a.md" }))), Times.Once);
    }

    [Fact]
    public async Task Delete_RecordsWithoutSuggestionsFiles_DoesNotCallStorage()
    {
        // Arrange
        _crudServiceMock
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([new QualityData { Id = "r1" }]);
        var service = CreateService();

        // Act
        await service.DeleteAsync(["r1"]);

        // Assert
        _crudServiceMock.Verify(x => x.DeleteAsync(It.IsAny<IList<string>>(), It.IsAny<bool>()), Times.Once);
        _blobStorageMock.Verify(x => x.RemoveAsync(It.IsAny<string[]>()), Times.Never);
    }

    /// <summary>
    /// MemoryStream that hands its content over when closed — like a blob being committed.
    /// </summary>
    private sealed class CapturingStream(Action<byte[]> onClose) : MemoryStream
    {
        private bool _closed;

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_closed)
            {
                _closed = true;
                onClose(ToArray());
            }

            base.Dispose(disposing);
        }
    }
}

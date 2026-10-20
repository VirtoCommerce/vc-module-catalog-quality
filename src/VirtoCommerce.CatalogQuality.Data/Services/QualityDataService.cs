using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Models.Search;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.CatalogQuality.Data.Repositories;

namespace VirtoCommerce.CatalogQuality.Data.Services;

public class QualityDataService : IQualityDataService
{
    private const string SuggestionsFolder = "catalog-quality/suggestions";

    private readonly IQualityDataCrudService _qualityDataCrudService;
    private readonly IQualityDataSearchService _qualityDataSearchService;
    private readonly IBlobStorageProvider _blobStorageProvider;
    private readonly Func<ICatalogQualityRepository> _repositoryFactory;

    public QualityDataService(
        IQualityDataCrudService qualityDataCrudService,
        IQualityDataSearchService qualityDataSearchService,
        IBlobStorageProvider blobStorageProvider,
        Func<ICatalogQualityRepository> repositoryFactory
        )
    {
        _qualityDataCrudService = qualityDataCrudService;
        _qualityDataSearchService = qualityDataSearchService;
        _blobStorageProvider = blobStorageProvider;
        _repositoryFactory = repositoryFactory;
    }

    public virtual async Task<SearchQualityDataResult> SearchAsync(SearchQualityDataCriteria criteria)
    {
        var result = await _qualityDataSearchService.SearchAsync(criteria);

        foreach (var model in result.Results.Where(x => !string.IsNullOrEmpty(x.SuggestionsUrl)))
        {
            model.Suggestions = await ReadSuggestionsAsync(model.SuggestionsUrl);
        }

        return result;
    }

    public virtual async Task<QualityData> GetQualityDataByEntityAsync(string entityId, string entityType, string targetId)
    {
        using var repository = _repositoryFactory();

        var qualityEntity = await repository.GetQualityDataByEntityAsync(entityId, entityType, targetId);

        var quality = qualityEntity?.ToModel(ExType<QualityData>.New());
        if (quality != null)
        {
            quality.Suggestions = await ReadSuggestionsAsync(quality.SuggestionsUrl);
        }

        return quality;
    }

    public virtual async Task SaveChangesAsync(IList<QualityData> models)
    {
        models = await ResolveExistingRecordsAsync(models);

        foreach (var model in models.Where(x => x.Suggestions != null))
        {
            model.SuggestionsUrl = await WriteSuggestionsAsync(model);
        }

        await _qualityDataCrudService.SaveChangesAsync(models);
    }

    public virtual async Task DeleteAsync(IList<string> ids)
    {
        var models = await _qualityDataCrudService.GetAsync(ids);
        var suggestionsUrls = models
            .Select(x => x.SuggestionsUrl)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        await _qualityDataCrudService.DeleteAsync(ids);

        if (suggestionsUrls.Length > 0)
        {
            await _blobStorageProvider.RemoveAsync(suggestionsUrls);
        }
    }

    protected virtual async Task<IList<QualityData>> ResolveExistingRecordsAsync(IList<QualityData> models)
    {
        var result = models
            .GroupBy(x => GetRecordKey(x.EntityId, x.EntityType, x.TargetId), StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Last())
            .ToList();

        var newModels = result.Where(x => x.IsTransient()).ToList();
        if (newModels.Count == 0)
        {
            return result;
        }

        using var repository = _repositoryFactory();

        var existingEntities = await repository.GetQualityDatasByEntitiesAsync(
            newModels.Select(x => x.EntityId).Distinct().ToList(),
            newModels.Select(x => x.EntityType).Distinct().ToList());

        var existingIds = existingEntities
            .GroupBy(x => GetRecordKey(x.EntityId, x.EntityType, x.TargetId), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First().Id, StringComparer.OrdinalIgnoreCase);

        foreach (var model in newModels)
        {
            if (existingIds.TryGetValue(GetRecordKey(model.EntityId, model.EntityType, model.TargetId), out var existingId))
            {
                model.Id = existingId;
            }
        }

        return result;
    }

    private static string GetRecordKey(string entityId, string entityType, string targetId)
    {
        return $"{entityType}|{entityId}|{(string.IsNullOrEmpty(targetId) ? null : targetId)}";
    }

    protected virtual string GetSuggestionsBlobUrl(QualityData model)
    {
        return string.IsNullOrEmpty(model.TargetId)
            ? $"{SuggestionsFolder}/{model.EntityType}/{model.EntityId}.md"
            : $"{SuggestionsFolder}/{model.EntityType}/{model.EntityId}/{model.TargetId}.md";
    }

    protected virtual async Task<string> WriteSuggestionsAsync(QualityData model)
    {
        var blobUrl = GetSuggestionsBlobUrl(model);

        await using var stream = await _blobStorageProvider.OpenWriteAsync(blobUrl);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(model.Suggestions);

        return blobUrl;
    }

    protected virtual async Task<string> ReadSuggestionsAsync(string blobUrl)
    {
        var blobInfo = await _blobStorageProvider.GetBlobInfoAsync(blobUrl);
        if (blobInfo == null)
        {
            return null;
        }

        await using var stream = await _blobStorageProvider.OpenReadAsync(blobUrl);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        return await reader.ReadToEndAsync();
    }
}

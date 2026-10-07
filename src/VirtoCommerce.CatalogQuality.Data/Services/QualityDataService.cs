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

    public virtual async Task<QualityData> GetQualityDataByEntityAsync(string entityId, string entityType)
    {
        using var repository = _repositoryFactory();

        var qualityEntity = await repository.GetQualityDataByEntityAsync(entityId, entityType);

        var quality = qualityEntity?.ToModel(ExType<QualityData>.New());
        if (quality != null)
        {
            quality.Suggestions = await ReadSuggestionsAsync(quality.SuggestionsUrl);
        }

        return quality;
    }

    public virtual async Task SaveChangesAsync(IList<QualityData> models)
    {
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

    protected virtual string GetSuggestionsBlobUrl(QualityData model)
    {
        return $"{SuggestionsFolder}/{model.EntityType}/{model.EntityId}.md";
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

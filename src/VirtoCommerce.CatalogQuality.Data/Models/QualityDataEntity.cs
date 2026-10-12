using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogQuality.Data.Models;

public class QualityDataEntity : AuditableEntity, IDataEntity<QualityDataEntity, QualityData>
{
    [Required]
    [StringLength(128)]
    public string EntityId { get; set; }

    [Required]
    [StringLength(128)]
    public string EntityType { get; set; }

    [StringLength(128)]
    public string TargetId { get; set; }

    public int ReadinessScore { get; set; }

    [StringLength(2083)]
    public string SuggestionsUrl { get; set; }

    public virtual QualityDataEntity FromModel(QualityData model, PrimaryKeyResolvingMap pkMap)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        EntityId = model.EntityId;
        EntityType = model.EntityType;
        TargetId = model.TargetId;
        ReadinessScore = model.ReadinessScore;
        SuggestionsUrl = model.SuggestionsUrl;

        return this;
    }

    public virtual QualityData ToModel(QualityData model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.EntityId = EntityId;
        model.EntityType = EntityType;
        model.TargetId = TargetId;
        model.ReadinessScore = ReadinessScore;
        model.SuggestionsUrl = SuggestionsUrl;

        return model;
    }

    public virtual void Patch(QualityDataEntity target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        target.EntityId = EntityId;
        target.EntityType = EntityType;
        target.TargetId = TargetId;
        target.ReadinessScore = ReadinessScore;
        target.SuggestionsUrl = SuggestionsUrl;
    }
}

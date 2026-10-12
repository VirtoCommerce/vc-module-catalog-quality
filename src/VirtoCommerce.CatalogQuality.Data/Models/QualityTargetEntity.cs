using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CatalogQuality.Data.Models;

public class QualityTargetEntity : AuditableEntity, IDataEntity<QualityTargetEntity, QualityTarget>
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; }

    [StringLength(1024)]
    public string Description { get; set; }

    public string Prompt { get; set; }

    public virtual QualityTargetEntity FromModel(QualityTarget model, PrimaryKeyResolvingMap pkMap)
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

        Name = model.Name;
        Description = model.Description;
        Prompt = model.Prompt;

        return this;
    }

    public virtual QualityTarget ToModel(QualityTarget model)
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

        model.Name = Name;
        model.Description = Description;
        model.Prompt = Prompt;

        return model;
    }

    public virtual void Patch(QualityTargetEntity target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        target.Name = Name;
        target.Description = Description;
        target.Prompt = Prompt;
    }
}

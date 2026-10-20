using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Events;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogQuality.Data.Commands;

public class UpdateQualityTargetCommandHandler : ICommandHandler<UpdateQualityTargetCommand>
{
    private readonly IQualityTargetCrudService _qualityTargetCrudService;
    private readonly IEventPublisher _eventPublisher;

    public UpdateQualityTargetCommandHandler(
        IQualityTargetCrudService qualityTargetCrudService,
        IEventPublisher eventPublisher
        )
    {
        _qualityTargetCrudService = qualityTargetCrudService;
        _eventPublisher = eventPublisher;
    }

    public virtual async Task Handle(UpdateQualityTargetCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.QualityTarget == null)
        {
            throw new ArgumentNullException(nameof(request.QualityTarget));
        }

        var qualityTarget = request.QualityTarget;

        var isModified = false;
        QualityTarget oldTargetvalue = null;
        if (!string.IsNullOrEmpty(qualityTarget.Id))
        {
            isModified = true;
            oldTargetvalue = await _qualityTargetCrudService.GetByIdAsync(qualityTarget.Id);
        }

        await _qualityTargetCrudService.SaveChangesAsync([qualityTarget]);

        if (isModified)
        {
            var changedEntry = new GenericChangedEntry<QualityTarget>(qualityTarget, oldTargetvalue, EntryState.Modified);
            await _eventPublisher.Publish(new TargetChangedEvent([changedEntry]));
        }
    }
}

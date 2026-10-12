using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Services;

namespace VirtoCommerce.CatalogQuality.Data.Commands;

public class UpdateQualityTargetCommandHandler : ICommandHandler<UpdateQualityTargetCommand>
{
    private readonly IQualityTargetCrudService _qualityTargetCrudService;

    public UpdateQualityTargetCommandHandler(
        IQualityTargetCrudService qualityTargetCrudService
        )
    {
        _qualityTargetCrudService = qualityTargetCrudService;
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

        await _qualityTargetCrudService.SaveChangesAsync([qualityTarget]);
    }
}

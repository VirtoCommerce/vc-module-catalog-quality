using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogQuality.Data.Commands;

public class DeleteQualityTargetCommandHandler : ICommandHandler<DeleteQualityTargetCommand>
{
    private readonly IQualityTargetCrudService _qualityTargetCrudService;

    public DeleteQualityTargetCommandHandler(
        IQualityTargetCrudService qualityTargetCrudService
        )
    {
        _qualityTargetCrudService = qualityTargetCrudService;
    }

    public virtual async Task Handle(DeleteQualityTargetCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Ids.IsNullOrEmpty())
        {
            return;
        }

        await _qualityTargetCrudService.DeleteAsync(request.Ids);
    }
}

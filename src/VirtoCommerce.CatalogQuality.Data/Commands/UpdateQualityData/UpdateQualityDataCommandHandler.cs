using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Services;

namespace VirtoCommerce.CatalogQuality.Data.Commands;

public class UpdateQualityDataCommandHandler : ICommandHandler<UpdateQualityDataCommand>
{
    private readonly IQualityDataService _qualityDataService;

    public UpdateQualityDataCommandHandler(
        IQualityDataService qualityDataService
        )
    {
        _qualityDataService = qualityDataService;
    }

    public virtual async Task Handle(UpdateQualityDataCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.QualityData == null)
        {
            throw new ArgumentNullException(nameof(request.QualityData));
        }

        var qualityData = request.QualityData;

        await _qualityDataService.SaveChangesAsync([qualityData]);
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogQuality.Data.Commands;

public class BulkUpdateQualityDataCommandHandler : ICommandHandler<BulkUpdateQualityDataCommand>
{
    private readonly IQualityDataService _qualityDataService;

    public BulkUpdateQualityDataCommandHandler(
        IQualityDataService qualityDataService
        )
    {
        _qualityDataService = qualityDataService;
    }

    public virtual async Task Handle(BulkUpdateQualityDataCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var qualityDatas = request.QualityDatas?.Where(x => x != null).ToList();
        if (qualityDatas.IsNullOrEmpty())
        {
            return;
        }

        await _qualityDataService.SaveChangesAsync(qualityDatas);
    }
}

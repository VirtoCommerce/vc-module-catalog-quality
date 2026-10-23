using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogQuality.Core.Common;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Core.Models.Search;
using VirtoCommerce.CatalogQuality.Data.Commands;
using VirtoCommerce.CatalogQuality.Data.Queries;
using Permissions = VirtoCommerce.CatalogQuality.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogQuality.Web.Controllers.Api;

[Authorize]
[Route("api/catalogquality/data")]
public class CatalogQualityDataController : Controller
{
    private readonly IMediator _mediator;

    public CatalogQualityDataController(
        IMediator mediator
        )
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Route("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<SearchQualityDataResult>> DataSearch([FromBody] SearchQualityDataQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet]
    [Route("getbyentity")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<QualityData>> DataGetByEntity([FromQuery] GetQualityDataQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Route("update")]
    [Authorize(Permissions.Update)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DataUpdate([FromBody] QualityData qualityData)
    {
        var command = ExType<UpdateQualityDataCommand>.New();
        command.QualityData = qualityData;

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost]
    [Route("bulkupdate")]
    [Authorize(Permissions.Update)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DataBulkUpdate([FromBody] QualityData[] qualityDatas)
    {
        var command = ExType<BulkUpdateQualityDataCommand>.New();
        command.QualityDatas = qualityDatas;

        await _mediator.Send(command);
        return NoContent();
    }
}

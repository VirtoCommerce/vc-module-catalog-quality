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
using VirtoCommerce.CatalogQuality.Data.Queries.GetQualityData;
using Permissions = VirtoCommerce.CatalogQuality.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogQuality.Web.Controllers.Api;

[Authorize]
[Route("api/catalogquality")]
public class CatalogQualityController : Controller
{
    private readonly IMediator _mediator;

    public CatalogQualityController(
        IMediator mediator
        )
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Route("data/search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<SearchQualityDataResult>> DataSearch([FromBody] SearchQualityDataQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet]
    [Route("data/getbyentity")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<QualityData>> DataGetByEntity([FromQuery] GetQualityDataQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Route("data/update")]
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
    [Route("data/bulkupdate")]
    [Authorize(Permissions.Update)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DataBulkUpdate([FromBody] QualityData[] qualityDatas)
    {
        var command = ExType<BulkUpdateQualityDataCommand>.New();
        command.QualityDatas = qualityDatas;

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost]
    [Route("target/search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<SearchQualityTargetResult>> TargetSearch([FromBody] SearchQualityTargetQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Route("target/update")]
    [Authorize(Permissions.Update)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> TargetUpdate([FromBody] QualityTarget qualityTarget)
    {
        var command = ExType<UpdateQualityTargetCommand>.New();
        command.QualityTarget = qualityTarget;

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete]
    [Route("target/delete")]
    [Authorize(Permissions.Delete)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> TargetDelete([FromQuery] string[] ids)
    {
        var command = ExType<DeleteQualityTargetCommand>.New();
        command.Ids = ids;

        await _mediator.Send(command);
        return NoContent();
    }
}

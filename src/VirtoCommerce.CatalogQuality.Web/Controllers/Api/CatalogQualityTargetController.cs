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
[Route("api/catalogquality/target")]
public class CatalogQualityTargetController : Controller
{
    private readonly IMediator _mediator;

    public CatalogQualityTargetController(
        IMediator mediator
        )
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Route("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<SearchQualityTargetResult>> TargetSearch([FromBody] SearchQualityTargetQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet]
    [Route("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<QualityTarget>> TargetGetById([FromRoute] GetQualityTargetQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Route("update")]
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
    [Route("delete")]
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

using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogQuality.Core.Models;
using VirtoCommerce.CatalogQuality.Data.Queries;
using Permissions = VirtoCommerce.CatalogQuality.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogQuality.Web.Controllers.Api;

[Authorize]
[Route("api/catalogquality/entitytypes")]
public class CatalogQualityEntityTypeController : Controller
{
    private readonly IMediator _mediator;

    public CatalogQualityEntityTypeController(
        IMediator mediator
        )
    {
        _mediator = mediator;
    }

    /// <summary>Entity types quality targets can be defined for, as registered by the installed modules</summary>
    [HttpGet]
    [Route("")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<IList<QualityEntityType>>> GetEntityTypes()
    {
        var result = await _mediator.Send(new GetQualityEntityTypesQuery());
        return Ok(result);
    }
}

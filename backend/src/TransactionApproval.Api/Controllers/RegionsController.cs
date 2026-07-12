using Microsoft.AspNetCore.Mvc;
using TransactionApproval.Application.DTOs;
using TransactionApproval.Application.Services;

namespace TransactionApproval.Api.Controllers;

[ApiController]
[Route("api/regions")]
[Produces("application/json")]
public class RegionsController : ControllerBase
{
    private readonly IRegionService _regionService;

    public RegionsController(IRegionService regionService)
    {
        _regionService = regionService;
    }

    /// <summary>Returns the fixed catalog of supported regions.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RegionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RegionDto>>> GetRegions(CancellationToken cancellationToken)
    {
        var regions = await _regionService.GetRegionsAsync(cancellationToken);
        return Ok(regions);
    }
}

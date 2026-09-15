using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PointsHistoryController : ControllerBase
{
    private readonly IPointsHistoryService _historyService;

    public PointsHistoryController(IPointsHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet("child/{childId}")]
    [ProducesResponseType(typeof(IEnumerable<PointsHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChildId(string childId, CancellationToken cancellationToken)
    {
        var result = await _historyService.GetByChildIdAsync(childId, cancellationToken);
        return Ok(result);
    }
}
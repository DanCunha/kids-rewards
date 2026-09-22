using KidsRewards.Api.Authentication;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PointsHistoryController : ControllerBase
{
    private readonly IPointsHistoryService _historyService;

    public PointsHistoryController(IPointsHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet("child/{childId}")]
    [ProducesResponseType(typeof(IEnumerable<PointsHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByChildId(string childId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _historyService.GetByChildIdAsync(childId, User.ToUserContext(), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

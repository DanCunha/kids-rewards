using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateActivityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _activityService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetByChildId), new { childId = result.ChildId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("child/{childId}")]
    [ProducesResponseType(typeof(IEnumerable<ActivityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChildId(string childId, CancellationToken cancellationToken)
    {
        var result = await _activityService.GetByChildIdAsync(childId, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(string id, CancellationToken cancellationToken)
    {
        var success = await _activityService.CompleteActivityAsync(id, cancellationToken);
        if (!success)
            return BadRequest(new { Message = "Não foi possível concluir a atividade. Ela pode já estar concluída ou não existir." });

        return Ok(new { Message = "Atividade concluída com sucesso e pontos adicionados ao saldo!" });
    }
}
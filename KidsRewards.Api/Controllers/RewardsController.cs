using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RewardsController : ControllerBase
{
    private readonly IRewardService _rewardService;

    public RewardsController(IRewardService rewardService)
    {
        _rewardService = rewardService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RewardResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateRewardRequest request, CancellationToken cancellationToken)
    {
        var result = await _rewardService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAllActive), result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RewardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(CancellationToken cancellationToken)
    {
        var result = await _rewardService.GetAllActiveAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<RewardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _rewardService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("redeem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Redeem([FromBody] RedeemRewardRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _rewardService.RedeemRewardAsync(request, cancellationToken);
            return Ok(new { Message = "Recompensa resgatada com sucesso!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActive(string id, [FromBody] UpdateRewardActiveRequest request, CancellationToken cancellationToken)
    {
        var updated = await _rewardService.SetActiveAsync(id, request, cancellationToken);
        if (!updated) return NotFound(new { Message = "Recompensa não encontrada." });
        return NoContent();
    }
}
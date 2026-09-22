using KidsRewards.Api.Authentication;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly IRewardService _rewardService;

    public RewardsController(IRewardService rewardService)
    {
        _rewardService = rewardService;
    }

    [HttpPost]
    [Authorize(Policy = AuthConstants.ResponsiblePolicy)]
    [ProducesResponseType(typeof(RewardResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateRewardRequest request, CancellationToken cancellationToken)
    {
        var result = await _rewardService.CreateAsync(User.ToUserContext(), request, cancellationToken);
        return CreatedAtAction(nameof(GetAllActive), result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RewardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(CancellationToken cancellationToken)
    {
        var result = await _rewardService.GetAllActiveAsync(User.ToUserContext(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Policy = AuthConstants.ResponsiblePolicy)]
    [ProducesResponseType(typeof(IEnumerable<RewardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _rewardService.GetAllAsync(User.ToUserContext(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("redeem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Redeem([FromBody] RedeemRewardRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _rewardService.RedeemRewardAsync(User.ToUserContext(), request, cancellationToken);
            return Ok(new { Message = "Recompensa resgatada com sucesso!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = AuthConstants.ResponsiblePolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActive(string id, [FromBody] UpdateRewardActiveRequest request, CancellationToken cancellationToken)
    {
        var updated = await _rewardService.SetActiveAsync(id, User.ToUserContext(), request, cancellationToken);
        if (!updated) return NotFound(new { Message = "Recompensa não encontrada." });
        return NoContent();
    }
}

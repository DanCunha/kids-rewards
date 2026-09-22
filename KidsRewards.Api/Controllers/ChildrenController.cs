using KidsRewards.Api.Authentication;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChildrenController : ControllerBase
{
    private readonly IChildService _childService;

    public ChildrenController(IChildService childService)
    {
        _childService = childService;
    }

    [HttpPost]
    [Authorize(Policy = AuthConstants.ResponsiblePolicy)]
    [ProducesResponseType(typeof(ChildResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateChildRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _childService.CreateAsync(User.ToUserContext(), request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ChildResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _childService.GetForUserAsync(User.ToUserContext(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ChildResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _childService.GetByIdAsync(id, User.ToUserContext(), cancellationToken);
        if (result == null) return NotFound(new { Message = $"Criança com ID {id} não encontrada." });
        return Ok(result);
    }

    [HttpPut("{id}/goal")]
    [Authorize(Policy = AuthConstants.ResponsiblePolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGoal(string id, [FromBody] UpdateChildGoalRequest request, CancellationToken cancellationToken)
    {
        var updated = await _childService.UpdateGoalAsync(id, User.ToUserContext(), request, cancellationToken);
        if (!updated) return NotFound(new { Message = "Criança não encontrada." });
        return NoContent();
    }
}

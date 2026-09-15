using KidsRewards.Api.DTOs;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KidsRewards.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChildrenController : ControllerBase
{
    private readonly IChildService _childService;

    public ChildrenController(IChildService childService)
    {
        _childService = childService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChildResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateChildRequest request, CancellationToken cancellationToken)
    {
        var result = await _childService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ChildResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _childService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ChildResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _childService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(new { Message = $"Criança com ID {id} não encontrada." });
        return Ok(result);
    }

    [HttpPut("{id}/goal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGoal(string id, [FromBody] UpdateChildGoalRequest request, CancellationToken cancellationToken)
    {
        var updated = await _childService.UpdateGoalAsync(id, request, cancellationToken);
        if (!updated) return NotFound(new { Message = "Criança não encontrada." });
        return NoContent();
    }
}
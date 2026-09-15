using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;

namespace KidsRewards.Api.Services.Implementation;

public class ChildService : IChildService
{
    private readonly IChildRepository _childRepository;

    public ChildService(IChildRepository childRepository)
    {
        _childRepository = childRepository;
    }

    public async Task<ChildResponse> CreateAsync(CreateChildRequest request, CancellationToken cancellationToken = default)
    {
        var child = new Child
        {
            Name = request.Name.Trim(),
            Age = request.Age,
            PointsBalance = 0,
            MonthlyGoalPoints = request.MonthlyGoalPoints,
            MonthlyGoalRewardId = request.MonthlyGoalRewardId,
            CreatedAt = DateTime.UtcNow
        };

        await _childRepository.CreateAsync(child, cancellationToken);
        return MapToResponse(child);
    }

    public async Task<IEnumerable<ChildResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var children = await _childRepository.GetAllAsync(cancellationToken);
        return children.Select(MapToResponse);
    }

    public async Task<ChildResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(id, cancellationToken);
        return child == null ? null : MapToResponse(child);
    }

    public async Task<bool> UpdateGoalAsync(string childId, UpdateChildGoalRequest request, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child == null) return false;

        return await _childRepository.UpdateGoalAsync(childId, request.MonthlyGoalPoints, request.MonthlyGoalRewardId, cancellationToken);
    }

    private static ChildResponse MapToResponse(Child c) =>
        new(c.Id, c.Name, c.Age, c.PointsBalance, c.MonthlyGoalPoints, c.MonthlyGoalRewardId, c.CreatedAt);
}
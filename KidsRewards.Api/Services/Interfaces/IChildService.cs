using KidsRewards.Api.DTOs;

namespace KidsRewards.Api.Services.Interfaces;

public interface IChildService
{
    Task<ChildResponse> CreateAsync(CreateChildRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChildResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ChildResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> UpdateGoalAsync(string childId, UpdateChildGoalRequest request, CancellationToken cancellationToken = default);
}
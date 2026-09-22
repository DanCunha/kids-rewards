using KidsRewards.Api.Authentication;
using KidsRewards.Api.DTOs;

namespace KidsRewards.Api.Services.Interfaces;

public interface IChildService
{
    Task<ChildResponse> CreateAsync(UserContext context, CreateChildRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChildResponse>> GetForUserAsync(UserContext context, CancellationToken cancellationToken = default);
    Task<ChildResponse?> GetByIdAsync(string id, UserContext context, CancellationToken cancellationToken = default);
    Task<bool> UpdateGoalAsync(string childId, UserContext context, UpdateChildGoalRequest request, CancellationToken cancellationToken = default);
}

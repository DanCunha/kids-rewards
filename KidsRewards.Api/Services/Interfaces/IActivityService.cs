using KidsRewards.Api.Authentication;
using KidsRewards.Api.DTOs;

namespace KidsRewards.Api.Services.Interfaces;

public interface IActivityService
{
    Task<ActivityResponse> CreateAsync(UserContext context, CreateActivityRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActivityResponse>> GetByChildIdAsync(string childId, UserContext context, CancellationToken cancellationToken = default);
    Task<bool> CompleteActivityAsync(string activityId, UserContext context, CancellationToken cancellationToken = default);
}

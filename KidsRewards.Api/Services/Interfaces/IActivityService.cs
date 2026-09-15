using KidsRewards.Api.DTOs;

namespace KidsRewards.Api.Services.Interfaces;

public interface IActivityService
{
    Task<ActivityResponse> CreateAsync(CreateActivityRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActivityResponse>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default);
    Task<bool> CompleteActivityAsync(string activityId, CancellationToken cancellationToken = default);
}
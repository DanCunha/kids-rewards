using KidsRewards.Api.Domain.Entities;

namespace KidsRewards.Api.Repositories.Interfaces;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Activity>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default);
    Task CreateAsync(Activity activity, CancellationToken cancellationToken = default);
    Task<bool> MarkAsCompletedAsync(string id, DateTime completedAt, CancellationToken cancellationToken = default);
}
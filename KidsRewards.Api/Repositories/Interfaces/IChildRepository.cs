using KidsRewards.Api.Domain.Entities;

namespace KidsRewards.Api.Repositories.Interfaces;

public interface IChildRepository
{
    Task<Child?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Child>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(Child child, CancellationToken cancellationToken = default);
    Task<bool> UpdateBalanceAsync(string childId, int deltaPoints, CancellationToken cancellationToken = default);
    Task<bool> UpdateGoalAsync(string childId, int monthlyGoalPoints, string? rewardId, CancellationToken cancellationToken = default);
}
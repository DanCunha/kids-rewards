using KidsRewards.Api.Domain.Entities;

namespace KidsRewards.Api.Repositories.Interfaces;

public interface IRewardRepository
{
    Task<Reward?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reward>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Reward>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(Reward reward, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(string id, bool isActive, CancellationToken cancellationToken = default);
}
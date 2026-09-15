using KidsRewards.Api.Domain.Entities;

namespace KidsRewards.Api.Repositories.Interfaces;

public interface IPointsHistoryRepository
{
    Task CreateAsync(PointsHistory history, CancellationToken cancellationToken = default);
    Task<IEnumerable<PointsHistory>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default);
}
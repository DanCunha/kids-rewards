using KidsRewards.Api.DTOs;

namespace KidsRewards.Api.Services.Interfaces;

public interface IPointsHistoryService
{
    Task<IEnumerable<PointsHistoryResponse>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default);
}
using KidsRewards.Api.Authentication;
using KidsRewards.Api.DTOs;

namespace KidsRewards.Api.Services.Interfaces;

public interface IPointsHistoryService
{
    Task<IEnumerable<PointsHistoryResponse>> GetByChildIdAsync(string childId, UserContext context, CancellationToken cancellationToken = default);
}

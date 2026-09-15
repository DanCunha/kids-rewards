using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;

namespace KidsRewards.Api.Services.Implementation;

public class PointsHistoryService : IPointsHistoryService
{
    private readonly IPointsHistoryRepository _historyRepository;

    public PointsHistoryService(IPointsHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<PointsHistoryResponse>> GetByChildIdAsync(string childId, CancellationToken cancellationToken = default)
    {
        var histories = await _historyRepository.GetByChildIdAsync(childId, cancellationToken);
        return histories.Select(h => new PointsHistoryResponse(
            h.Id,
            h.ChildId,
            h.Type.ToString(),
            h.Points,
            h.ReferenceId,
            h.Description,
            h.CreatedAt
        ));
    }
}
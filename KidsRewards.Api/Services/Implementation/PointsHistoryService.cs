using KidsRewards.Api.Authentication;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;

namespace KidsRewards.Api.Services.Implementation;

public class PointsHistoryService : IPointsHistoryService
{
    private readonly IPointsHistoryRepository _historyRepository;
    private readonly IChildRepository _childRepository;

    public PointsHistoryService(IPointsHistoryRepository historyRepository, IChildRepository childRepository)
    {
        _historyRepository = historyRepository;
        _childRepository = childRepository;
    }

    public async Task<IEnumerable<PointsHistoryResponse>> GetByChildIdAsync(string childId, UserContext context, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child == null || !CanAccess(child, context))
            throw new InvalidOperationException("Você não tem permissão para visualizar o histórico desta criança.");

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

    private static bool CanAccess(Domain.Entities.Child child, UserContext context)
    {
        if (context.IsChild) return context.ChildId == child.Id;
        return child.ResponsibleUserId == context.UserId;
    }
}

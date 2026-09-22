using KidsRewards.Api.Authentication;
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Domain.Enums;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;

namespace KidsRewards.Api.Services.Implementation;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _activityRepository;
    private readonly IChildRepository _childRepository;
    private readonly IPointsHistoryRepository _historyRepository;

    public ActivityService(
        IActivityRepository activityRepository,
        IChildRepository childRepository,
        IPointsHistoryRepository historyRepository)
    {
        _activityRepository = activityRepository;
        _childRepository = childRepository;
        _historyRepository = historyRepository;
    }

    public async Task<ActivityResponse> CreateAsync(UserContext context, CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(request.ChildId, cancellationToken);
        if (child == null)
            throw new InvalidOperationException($"Criança com ID '{request.ChildId}' não foi encontrada.");
        if (child.ResponsibleUserId != context.UserId)
            throw new InvalidOperationException("Você não tem permissão para gerenciar atividades desta criança.");

        var activity = new Activity
        {
            ChildId = request.ChildId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Points = request.Points,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _activityRepository.CreateAsync(activity, cancellationToken);
        return MapToResponse(activity);
    }

    public async Task<IEnumerable<ActivityResponse>> GetByChildIdAsync(string childId, UserContext context, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child == null || !CanAccess(child, context))
            throw new InvalidOperationException("Você não tem permissão para visualizar atividades desta criança.");

        var activities = await _activityRepository.GetByChildIdAsync(childId, cancellationToken);
        return activities.Select(MapToResponse);
    }

    public async Task<bool> CompleteActivityAsync(string activityId, UserContext context, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(activityId, cancellationToken);
        if (activity == null || activity.IsCompleted)
            return false;

        var child = await _childRepository.GetByIdAsync(activity.ChildId, cancellationToken);
        if (child == null || !CanAccess(child, context))
            return false;

        var now = DateTime.UtcNow;

        // 1. Marca atividade como concluída (filtro atômico impede dupla conclusão)
        var marked = await _activityRepository.MarkAsCompletedAsync(activityId, now, cancellationToken);
        if (!marked) return false;

        // 2. Incrementa pontos da criança (soma atômica)
        await _childRepository.UpdateBalanceAsync(activity.ChildId, activity.Points, cancellationToken);

        // 3. Grava histórico imutável
        var history = new PointsHistory
        {
            ChildId = activity.ChildId,
            Type = PointOperationType.Earned,
            Points = activity.Points,
            ReferenceId = activity.Id,
            Description = $"Atividade Concluída: {activity.Title}",
            CreatedAt = now
        };
        await _historyRepository.CreateAsync(history, cancellationToken);

        return true;
    }

    private static bool CanAccess(Child child, UserContext context)
    {
        if (context.IsChild) return context.ChildId == child.Id;
        return child.ResponsibleUserId == context.UserId;
    }

    private static ActivityResponse MapToResponse(Activity a) =>
        new(a.Id, a.ChildId, a.Title, a.Description, a.Points, a.IsCompleted, a.CompletedAt, a.CreatedAt);
}

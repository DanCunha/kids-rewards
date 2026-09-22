using KidsRewards.Api.Authentication;
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Domain.Enums;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;

namespace KidsRewards.Api.Services.Implementation;

public class RewardService : IRewardService
{
    private readonly IRewardRepository _rewardRepository;
    private readonly IChildRepository _childRepository;
    private readonly IPointsHistoryRepository _historyRepository;

    public RewardService(
        IRewardRepository rewardRepository,
        IChildRepository childRepository,
        IPointsHistoryRepository historyRepository)
    {
        _rewardRepository = rewardRepository;
        _childRepository = childRepository;
        _historyRepository = historyRepository;
    }

    public async Task<RewardResponse> CreateAsync(UserContext context, CreateRewardRequest request, CancellationToken cancellationToken = default)
    {
        var reward = new Reward
        {
            Name = request.Name.Trim(),
            RequiredPoints = request.RequiredPoints,
            OwnerUserId = context.UserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _rewardRepository.CreateAsync(reward, cancellationToken);
        return MapToResponse(reward);
    }

    public async Task<IEnumerable<RewardResponse>> GetAllAsync(UserContext context, CancellationToken cancellationToken = default)
    {
        var rewards = await _rewardRepository.GetByOwnerAsync(context.UserId, cancellationToken);
        return rewards.Select(MapToResponse);
    }

    public async Task<IEnumerable<RewardResponse>> GetAllActiveAsync(UserContext context, CancellationToken cancellationToken = default)
    {
        var ownerUserId = await ResolveOwnerAsync(context, cancellationToken);
        if (ownerUserId == null) return Array.Empty<RewardResponse>();

        var rewards = await _rewardRepository.GetActiveByOwnerAsync(ownerUserId, cancellationToken);
        return rewards.Select(MapToResponse);
    }

    public async Task<bool> RedeemRewardAsync(UserContext context, RedeemRewardRequest request, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(request.ChildId, cancellationToken);
        if (child == null)
            throw new InvalidOperationException("Criança não encontrada.");
        if (!CanAccess(child, context))
            throw new InvalidOperationException("Você não tem permissão para resgatar pontos desta criança.");

        var reward = await _rewardRepository.GetByIdAsync(request.RewardId, cancellationToken);
        if (reward == null || !reward.IsActive)
            throw new InvalidOperationException("Recompensa inválida ou inativa.");
        if (reward.OwnerUserId != child.ResponsibleUserId)
            throw new InvalidOperationException("Recompensa não pertence ao responsável desta criança.");

        // REGRA DE NEGÓCIO: verificar saldo antes de debitar
        if (child.PointsBalance < reward.RequiredPoints)
            throw new InvalidOperationException($"Saldo insuficiente. Pontos atuais: {child.PointsBalance}, Necessários: {reward.RequiredPoints}");

        // 1. Débito atômico com filtro Gte no Mongo (impede saldo negativo)
        var updated = await _childRepository.UpdateBalanceAsync(request.ChildId, -reward.RequiredPoints, cancellationToken);
        if (!updated)
            throw new InvalidOperationException("Falha ao debitar pontos. Verifique o saldo disponível.");

        // 2. Registra o histórico de resgate
        var history = new PointsHistory
        {
            ChildId = request.ChildId,
            Type = PointOperationType.Spent,
            Points = reward.RequiredPoints,
            ReferenceId = reward.Id,
            Description = $"Recompensa Resgatada: {reward.Name}",
            CreatedAt = DateTime.UtcNow
        };

        await _historyRepository.CreateAsync(history, cancellationToken);

        return true;
    }

    public async Task<bool> SetActiveAsync(string rewardId, UserContext context, UpdateRewardActiveRequest request, CancellationToken cancellationToken = default)
    {
        var reward = await _rewardRepository.GetByIdAsync(rewardId, cancellationToken);
        if (reward == null || reward.OwnerUserId != context.UserId)
            return false;

        return await _rewardRepository.SetActiveAsync(rewardId, request.IsActive, cancellationToken);
    }

    private async Task<string?> ResolveOwnerAsync(UserContext context, CancellationToken cancellationToken)
    {
        if (context.IsChild)
        {
            var child = context.ChildId == null ? null : await _childRepository.GetByIdAsync(context.ChildId, cancellationToken);
            return child?.ResponsibleUserId;
        }

        return context.UserId;
    }

    private static bool CanAccess(Child child, UserContext context)
    {
        if (context.IsChild) return context.ChildId == child.Id;
        return child.ResponsibleUserId == context.UserId;
    }

    private static RewardResponse MapToResponse(Reward r) =>
        new(r.Id, r.Name, r.RequiredPoints, r.IsActive, r.CreatedAt);
}

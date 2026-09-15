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

    public async Task<RewardResponse> CreateAsync(CreateRewardRequest request, CancellationToken cancellationToken = default)
    {
        var reward = new Reward
        {
            Name = request.Name.Trim(),
            RequiredPoints = request.RequiredPoints,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _rewardRepository.CreateAsync(reward, cancellationToken);
        return MapToResponse(reward);
    }

    public async Task<IEnumerable<RewardResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rewards = await _rewardRepository.GetAllAsync(cancellationToken);
        return rewards.Select(MapToResponse);
    }

    public async Task<IEnumerable<RewardResponse>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var rewards = await _rewardRepository.GetAllActiveAsync(cancellationToken);
        return rewards.Select(MapToResponse);
    }

    public async Task<bool> RedeemRewardAsync(RedeemRewardRequest request, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(request.ChildId, cancellationToken);
        if (child == null)
            throw new InvalidOperationException("Criança não encontrada.");

        var reward = await _rewardRepository.GetByIdAsync(request.RewardId, cancellationToken);
        if (reward == null || !reward.IsActive)
            throw new InvalidOperationException("Recompensa inválida ou inativa.");

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

    public async Task<bool> SetActiveAsync(string rewardId, UpdateRewardActiveRequest request, CancellationToken cancellationToken = default)
    {
        return await _rewardRepository.SetActiveAsync(rewardId, request.IsActive, cancellationToken);
    }

    private static RewardResponse MapToResponse(Reward r) =>
        new(r.Id, r.Name, r.RequiredPoints, r.IsActive, r.CreatedAt);
}
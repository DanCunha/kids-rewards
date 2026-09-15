using KidsRewards.Api.DTOs;

namespace KidsRewards.Api.Services.Interfaces;

public interface IRewardService
{
    Task<RewardResponse> CreateAsync(CreateRewardRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<RewardResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RewardResponse>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> RedeemRewardAsync(RedeemRewardRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(string rewardId, UpdateRewardActiveRequest request, CancellationToken cancellationToken = default);
}
using KidsRewards.Api.Authentication;
using KidsRewards.Api.DTOs;

namespace KidsRewards.Api.Services.Interfaces;

public interface IRewardService
{
    Task<RewardResponse> CreateAsync(UserContext context, CreateRewardRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<RewardResponse>> GetAllAsync(UserContext context, CancellationToken cancellationToken = default);
    Task<IEnumerable<RewardResponse>> GetAllActiveAsync(UserContext context, CancellationToken cancellationToken = default);
    Task<bool> RedeemRewardAsync(UserContext context, RedeemRewardRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(string rewardId, UserContext context, UpdateRewardActiveRequest request, CancellationToken cancellationToken = default);
}

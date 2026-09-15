using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Domain.Enums;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Implementation;
using Moq;

namespace KidsRewards.Api.Tests.Services;

public class RewardServiceTests
{
    private readonly Mock<IRewardRepository> _rewardRepository;
    private readonly Mock<IChildRepository> _childRepository;
    private readonly Mock<IPointsHistoryRepository> _historyRepository;
    private readonly RewardService _service;

    public RewardServiceTests()
    {
        _rewardRepository = new Mock<IRewardRepository>();
        _childRepository = new Mock<IChildRepository>();
        _historyRepository = new Mock<IPointsHistoryRepository>();
        _service = new RewardService(_rewardRepository.Object, _childRepository.Object, _historyRepository.Object);
    }

    [Fact]
    public async Task RedeemRewardAsync_WithSufficientBalance_DebitsAndWritesSpentHistory()
    {
        var child = new Child { Id = "c1", Name = "Lucas", PointsBalance = 100 };
        var reward = new Reward { Id = "r1", Name = "Video Game", RequiredPoints = 50, IsActive = true };
        _childRepository.Setup(r => r.GetByIdAsync("c1", It.IsAny<CancellationToken>())).ReturnsAsync(child);
        _rewardRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>())).ReturnsAsync(reward);
        _childRepository.Setup(r => r.UpdateBalanceAsync("c1", -50, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var request = new RedeemRewardRequest("c1", "r1");
        var result = await _service.RedeemRewardAsync(request, CancellationToken.None);

        Assert.True(result);
        _childRepository.Verify(r => r.UpdateBalanceAsync("c1", -50, It.IsAny<CancellationToken>()), Times.Once);
        _historyRepository.Verify(
            r => r.CreateAsync(
                It.Is<PointsHistory>(h =>
                    h.Type == PointOperationType.Spent &&
                    h.Points == 50 &&
                    h.ReferenceId == "r1" &&
                    h.Description == "Recompensa Resgatada: Video Game"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RedeemRewardAsync_WithInsufficientBalance_ThrowsWithoutDebit()
    {
        var child = new Child { Id = "c1", Name = "Lucas", PointsBalance = 30 };
        var reward = new Reward { Id = "r1", Name = "Video Game", RequiredPoints = 50, IsActive = true };
        _childRepository.Setup(r => r.GetByIdAsync("c1", It.IsAny<CancellationToken>())).ReturnsAsync(child);
        _rewardRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>())).ReturnsAsync(reward);

        var request = new RedeemRewardRequest("c1", "r1");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RedeemRewardAsync(request, CancellationToken.None));
        Assert.Contains("Saldo insuficiente", ex.Message);
        _childRepository.Verify(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _historyRepository.Verify(r => r.CreateAsync(It.IsAny<PointsHistory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RedeemRewardAsync_WhenChildNotFound_Throws()
    {
        _childRepository.Setup(r => r.GetByIdAsync("inexistente", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Child?)null);

        var request = new RedeemRewardRequest("inexistente", "r1");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RedeemRewardAsync(request, CancellationToken.None));
    }

    [Theory]
    [InlineData(false)]   // recompensa inativa
    [InlineData(true)]    // recompensa inexistente
    public async Task RedeemRewardAsync_WhenRewardInvalid_Throws(bool useNullReward)
    {
        _childRepository.Setup(r => r.GetByIdAsync("c1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Child { Id = "c1", Name = "Lucas", PointsBalance = 100 });
        Reward? reward = useNullReward ? null : new Reward { Id = "r1", Name = "Video Game", RequiredPoints = 50, IsActive = false };
        _rewardRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>())).ReturnsAsync(reward);

        var request = new RedeemRewardRequest("c1", "r1");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RedeemRewardAsync(request, CancellationToken.None));
        Assert.Contains("Recompensa inválida ou inativa", ex.Message);
        _childRepository.Verify(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RedeemRewardAsync_WhenAtomicDebitFails_ThrowsWithoutHistory()
    {
        var child = new Child { Id = "c1", Name = "Lucas", PointsBalance = 100 };
        var reward = new Reward { Id = "r1", Name = "Video Game", RequiredPoints = 50, IsActive = true };
        _childRepository.Setup(r => r.GetByIdAsync("c1", It.IsAny<CancellationToken>())).ReturnsAsync(child);
        _rewardRepository.Setup(r => r.GetByIdAsync("r1", It.IsAny<CancellationToken>())).ReturnsAsync(reward);
        _childRepository.Setup(r => r.UpdateBalanceAsync("c1", -50, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var request = new RedeemRewardRequest("c1", "r1");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RedeemRewardAsync(request, CancellationToken.None));
        _historyRepository.Verify(r => r.CreateAsync(It.IsAny<PointsHistory>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
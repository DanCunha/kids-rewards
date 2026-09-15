using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Implementation;
using Moq;

namespace KidsRewards.Api.Tests.Services;

public class ChildServiceTests
{
    private readonly Mock<IChildRepository> _childRepository;
    private readonly ChildService _service;

    public ChildServiceTests()
    {
        _childRepository = new Mock<IChildRepository>();
        _service = new ChildService(_childRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_SetsBalanceZeroAndTrimsName()
    {
        var request = new CreateChildRequest("  Lucas  ", 8, 200, null);

        var result = await _service.CreateAsync(request, CancellationToken.None);

        Assert.Equal("Lucas", result.Name);
        Assert.Equal(8, result.Age);
        Assert.Equal(0, result.PointsBalance);
        Assert.Equal(200, result.MonthlyGoalPoints);
        _childRepository.Verify(
            r => r.CreateAsync(
                It.Is<Domain.Entities.Child>(c => c.Name == "Lucas" && c.PointsBalance == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_StartsWithZeroMonthlyGoal_WhenNotProvided()
    {
        var request = new CreateChildRequest("Ana", 5);

        var result = await _service.CreateAsync(request, CancellationToken.None);

        Assert.Equal(0, result.MonthlyGoalPoints);
        Assert.Null(result.MonthlyGoalRewardId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _childRepository.Setup(r => r.GetByIdAsync("inexistente", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Child?)null);

        var result = await _service.GetByIdAsync("inexistente", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateGoalAsync_WhenChildNotFound_ReturnsFalse()
    {
        _childRepository.Setup(r => r.GetByIdAsync("inexistente", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Child?)null);

        var result = await _service.UpdateGoalAsync("inexistente", new UpdateChildGoalRequest(300, null), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateGoalAsync_PersistsGoal()
    {
        _childRepository.Setup(r => r.GetByIdAsync("c1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Child { Id = "c1", Name = "Lucas" });
        _childRepository.Setup(r => r.UpdateGoalAsync("c1", 300, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateGoalAsync("c1", new UpdateChildGoalRequest(300, null), CancellationToken.None);

        Assert.True(result);
        _childRepository.Verify(r => r.UpdateGoalAsync("c1", 300, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
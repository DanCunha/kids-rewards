using KidsRewards.Api.Authentication;
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Implementation;
using Moq;

namespace KidsRewards.Api.Tests.Services;

public class ChildServiceTests
{
    private readonly Mock<IChildRepository> _childRepository;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly ChildService _service;
    private readonly UserContext _context = new("u1", "Responsible", null);

    public ChildServiceTests()
    {
        _childRepository = new Mock<IChildRepository>();
        _userRepository = new Mock<IUserRepository>();
        _service = new ChildService(_childRepository.Object, _userRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_SetsBalanceZeroAndTrimsName()
    {
        var request = new CreateChildRequest("  Lucas  ", 8, 200, null);

        var result = await _service.CreateAsync(_context, request, CancellationToken.None);

        Assert.Equal("Lucas", result.Name);
        Assert.Equal(8, result.Age);
        Assert.Equal(0, result.PointsBalance);
        Assert.Equal(200, result.MonthlyGoalPoints);
        _childRepository.Verify(
            r => r.CreateAsync(
                It.Is<Child>(c => c.Name == "Lucas" && c.PointsBalance == 0 && c.ResponsibleUserId == "u1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_StartsWithZeroMonthlyGoal_WhenNotProvided()
    {
        var request = new CreateChildRequest("Ana", 5);

        var result = await _service.CreateAsync(_context, request, CancellationToken.None);

        Assert.Equal(0, result.MonthlyGoalPoints);
        Assert.Null(result.MonthlyGoalRewardId);
    }

    [Fact]
    public async Task CreateAsync_WithChildLogin_CreatesLinkedChildUser()
    {
        var request = new CreateChildRequest("Ana", 5, 0, null, "ana@email.com", "secret123");

        await _service.CreateAsync(_context, request, CancellationToken.None);

        _userRepository.Verify(
            r => r.CreateAsync(
                It.Is<User>(u => u.Type == Domain.Enums.UserType.Child && u.ChildId != null && u.Email == "ana@email.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _childRepository.Setup(r => r.GetByIdAsync("inexistente", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Child?)null);

        var result = await _service.GetByIdAsync("inexistente", _context, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotOwned_ReturnsNull()
    {
        _childRepository.Setup(r => r.GetByIdAsync("c1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Child { Id = "c1", Name = "Lucas", ResponsibleUserId = "outro" });

        var result = await _service.GetByIdAsync("c1", _context, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateGoalAsync_WhenChildNotFound_ReturnsFalse()
    {
        _childRepository.Setup(r => r.GetByIdAsync("inexistente", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Child?)null);

        var result = await _service.UpdateGoalAsync("inexistente", _context, new UpdateChildGoalRequest(300, null), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateGoalAsync_PersistsGoal()
    {
        _childRepository.Setup(r => r.GetByIdAsync("c1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Child { Id = "c1", Name = "Lucas", ResponsibleUserId = "u1" });
        _childRepository.Setup(r => r.UpdateGoalAsync("c1", 300, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateGoalAsync("c1", _context, new UpdateChildGoalRequest(300, null), CancellationToken.None);

        Assert.True(result);
        _childRepository.Verify(r => r.UpdateGoalAsync("c1", 300, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}

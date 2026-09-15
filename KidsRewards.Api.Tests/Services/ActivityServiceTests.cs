using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Domain.Enums;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Implementation;
using Moq;

namespace KidsRewards.Api.Tests.Services;

public class ActivityServiceTests
{
    private readonly Mock<IActivityRepository> _activityRepository;
    private readonly Mock<IChildRepository> _childRepository;
    private readonly Mock<IPointsHistoryRepository> _historyRepository;
    private readonly ActivityService _service;

    public ActivityServiceTests()
    {
        _activityRepository = new Mock<IActivityRepository>();
        _childRepository = new Mock<IChildRepository>();
        _historyRepository = new Mock<IPointsHistoryRepository>();
        _service = new ActivityService(_activityRepository.Object, _childRepository.Object, _historyRepository.Object);
    }

    [Fact]
    public async Task CompleteActivityAsync_PendingActivity_AddsPointsAndWritesHistory()
    {
        var activity = new Activity
        {
            Id = "a1",
            ChildId = "c1",
            Title = "Arrumar a cama",
            Points = 15,
            IsCompleted = false
        };
        _activityRepository.Setup(r => r.GetByIdAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _activityRepository.Setup(r => r.MarkAsCompletedAsync("a1", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _childRepository.Setup(r => r.UpdateBalanceAsync("c1", 15, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _service.CompleteActivityAsync("a1", CancellationToken.None);

        Assert.True(result);
        _childRepository.Verify(r => r.UpdateBalanceAsync("c1", 15, It.IsAny<CancellationToken>()), Times.Once);
        _historyRepository.Verify(
            r => r.CreateAsync(
                It.Is<PointsHistory>(h =>
                    h.Type == PointOperationType.Earned &&
                    h.Points == 15 &&
                    h.ReferenceId == "a1" &&
                    h.Description == "Atividade Concluída: Arrumar a cama"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteActivityAsync_AlreadyCompleted_ReturnsFalseWithoutSideEffects()
    {
        var activity = new Activity
        {
            Id = "a1",
            ChildId = "c1",
            Title = "Arrumar a cama",
            Points = 15,
            IsCompleted = true
        };
        _activityRepository.Setup(r => r.GetByIdAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(activity);

        var result = await _service.CompleteActivityAsync("a1", CancellationToken.None);

        Assert.False(result);
        _childRepository.Verify(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _historyRepository.Verify(r => r.CreateAsync(It.IsAny<PointsHistory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CompleteActivityAsync_WhenActivityNotFound_ReturnsFalse()
    {
        _activityRepository.Setup(r => r.GetByIdAsync("inexistente", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await _service.CompleteActivityAsync("inexistente", CancellationToken.None);

        Assert.False(result);
        _childRepository.Verify(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CompleteActivityAsync_WhenMarkFails_DoesNotAddPointsOrWriteHistory()
    {
        var activity = new Activity
        {
            Id = "a1",
            ChildId = "c1",
            Title = "Arrumar a cama",
            Points = 15,
            IsCompleted = false
        };
        _activityRepository.Setup(r => r.GetByIdAsync("a1", It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _activityRepository.Setup(r => r.MarkAsCompletedAsync("a1", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CompleteActivityAsync("a1", CancellationToken.None);

        Assert.False(result);
        _childRepository.Verify(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _historyRepository.Verify(r => r.CreateAsync(It.IsAny<PointsHistory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenChildNotFound_Throws()
    {
        _childRepository.Setup(r => r.GetByIdAsync("inexistente", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Child?)null);
        var request = new CreateActivityRequest("inexistente", "Lição de casa", "Matemática", 30);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_CreatesPendingActivity()
    {
        _childRepository.Setup(r => r.GetByIdAsync("c1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Child { Id = "c1", Name = "Lucas" });
        var request = new CreateActivityRequest("c1", "  Lição de casa  ", "Matemática", 30);

        var result = await _service.CreateAsync(request, CancellationToken.None);

        Assert.Equal("Lição de casa", result.Title);
        Assert.Equal(30, result.Points);
        Assert.False(result.IsCompleted);
        _activityRepository.Verify(
            r => r.CreateAsync(
                It.Is<Activity>(a => a.ChildId == "c1" && a.Title == "Lição de casa" && a.Points == 30 && !a.IsCompleted),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
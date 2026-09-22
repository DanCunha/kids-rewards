using KidsRewards.Api.Configuration;
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Domain.Enums;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Implementation;
using Microsoft.Extensions.Options;
using Moq;

namespace KidsRewards.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        var jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "test-secret-with-at-least-32-characters!!",
            Issuer = "KidsRewards.Api",
            Audience = "KidsRewards.Frontend",
            ExpirationMinutes = 60
        });
        _service = new AuthService(_userRepository.Object, jwtSettings);
    }

    [Fact]
    public async Task RegisterAsync_HashesPasswordAndCreatesResponsible()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("pai@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        User? created = null;
        _userRepository.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => created = u);
        var request = new RegisterRequest("Pai", "pai@email.com", "secret123");

        var result = await _service.RegisterAsync(request, CancellationToken.None);

        Assert.Equal("Responsible", result.Type);
        Assert.Equal("pai@email.com", result.Email);
        Assert.NotNull(created);
        Assert.Equal(UserType.Responsible, created!.Type);
        Assert.NotEqual("secret123", created.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("secret123", created.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_Throws()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("pai@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = "u1", Email = "pai@email.com" });

        var request = new RegisterRequest("Pai", "pai@email.com", "secret123");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = new User
        {
            Id = "u1",
            Name = "Pai",
            Email = "pai@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret123"),
            Type = UserType.Responsible
        };
        _userRepository.Setup(r => r.GetByEmailAsync("pai@email.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _service.LoginAsync(new LoginRequest("pai@email.com", "secret123"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.Token));
        Assert.Equal("u1", result.User.Id);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        var user = new User
        {
            Id = "u1",
            Email = "pai@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret123"),
            Type = UserType.Responsible
        };
        _userRepository.Setup(r => r.GetByEmailAsync("pai@email.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _service.LoginAsync(new LoginRequest("pai@email.com", "senha-errada"), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ReturnsNull()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("nada@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _service.LoginAsync(new LoginRequest("nada@email.com", "secret123"), CancellationToken.None);

        Assert.Null(result);
    }
}

using KidsRewards.Api.Configuration;
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Domain.Enums;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Implementation;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using Moq;

namespace KidsRewards.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepository;
    private readonly Mock<IEmailService> _emailService;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _passwordResetTokenRepository = new Mock<IPasswordResetTokenRepository>();
        _emailService = new Mock<IEmailService>();
        var jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "test-secret-with-at-least-32-characters!!",
            Issuer = "KidsRewards.Api",
            Audience = "KidsRewards.Frontend",
            ExpirationMinutes = 60
        });
        var passwordResetSettings = Options.Create(new PasswordResetSettings
        {
            TokenExpirationMinutes = 30,
            FrontendBaseUrl = "http://localhost:3000"
        });
        _service = new AuthService(
            _userRepository.Object,
            _passwordResetTokenRepository.Object,
            _emailService.Object,
            jwtSettings,
            passwordResetSettings);
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

    [Fact]
    public async Task RequestPasswordResetAsync_WhenUserExists_CreatesTokenAndSendsEmail()
    {
        var user = new User { Id = "u1", Name = "Pai", Email = "pai@email.com" };
        _userRepository.Setup(r => r.GetByEmailAsync("pai@email.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await _service.RequestPasswordResetAsync(new ForgotPasswordRequest("pai@email.com"), CancellationToken.None);

        _passwordResetTokenRepository.Verify(r => r.DeleteByUserIdAsync("u1", It.IsAny<CancellationToken>()), Times.Once);
        _passwordResetTokenRepository.Verify(r => r.CreateAsync(It.Is<PasswordResetToken>(t => t.UserId == "u1" && t.ExpiresAt > t.CreatedAt), It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendAsync("pai@email.com", It.IsAny<string>(), It.Is<string>(b => b.Contains("/reset-password?token=")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_WhenUserNotFound_DoesNothing()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("nada@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await _service.RequestPasswordResetAsync(new ForgotPasswordRequest("nada@email.com"), CancellationToken.None);

        _passwordResetTokenRepository.Verify(r => r.CreateAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailService.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_UpdatesPasswordAndMarksUsed()
    {
        var resetToken = new PasswordResetToken
        {
            Id = "rt1",
            UserId = "u1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _passwordResetTokenRepository.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(resetToken);
        _userRepository.Setup(r => r.GetByIdAsync("u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = "u1", Email = "pai@email.com", PasswordHash = "old-hash" });
        string? updatedHash = null;
        _userRepository.Setup(r => r.UpdatePasswordHashAsync("u1", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, hash, _) => updatedHash = hash)
            .ReturnsAsync(true);

        await _service.ResetPasswordAsync(new ResetPasswordRequest("token", "nova-senha"), CancellationToken.None);

        Assert.NotNull(updatedHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("nova-senha", updatedHash));
        _passwordResetTokenRepository.Verify(r => r.MarkUsedAsync("rt1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenTokenNotFound_Throws()
    {
        _passwordResetTokenRepository.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ResetPasswordAsync(new ResetPasswordRequest("token", "nova-senha"), CancellationToken.None));
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenTokenExpired_Throws()
    {
        var resetToken = new PasswordResetToken
        {
            Id = "rt1",
            UserId = "u1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };
        _passwordResetTokenRepository.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(resetToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ResetPasswordAsync(new ResetPasswordRequest("token", "nova-senha"), CancellationToken.None));
    }
}

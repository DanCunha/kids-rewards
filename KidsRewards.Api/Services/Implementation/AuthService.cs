using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KidsRewards.Api.Configuration;
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Domain.Enums;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KidsRewards.Api.Services.Implementation;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;
    private readonly PasswordResetSettings _passwordResetSettings;

    public AuthService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailService emailService,
        IOptions<JwtSettings> jwtSettings,
        IOptions<PasswordResetSettings> passwordResetSettings)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailService = emailService;
        _jwtSettings = jwtSettings.Value;
        _passwordResetSettings = passwordResetSettings.Value;
    }

    public async Task<AuthUserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException("Já existe uma conta com este email.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Type = UserType.Responsible,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, cancellationToken);
        return MapToResponse(user);
    }

    public async Task<AuthLoginResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return new AuthLoginResult(GenerateToken(user), MapToResponse(user));
    }

    public async Task<AuthUserResponse?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user == null ? null : MapToResponse(user);
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
            return;

        var token = GenerateResetToken();
        var now = DateTime.UtcNow;

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(token),
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(_passwordResetSettings.TokenExpirationMinutes)
        };

        await _passwordResetTokenRepository.DeleteByUserIdAsync(user.Id, cancellationToken);
        await _passwordResetTokenRepository.CreateAsync(resetToken, cancellationToken);

        var resetLink = $"{_passwordResetSettings.FrontendBaseUrl.TrimEnd('/')}/reset-password?token={token}";
        await _emailService.SendAsync(
            user.Email,
            "Redefinição de senha — KidsRewards",
            $"<p>Olá, {user.Name}.</p><p>Para redefinir sua senha, clique no link abaixo:</p><p><a href=\"{resetLink}\">Redefinir senha</a></p><p>Se você não solicitou esta redefinição, ignore este email.</p>",
            cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var resetToken = await _passwordResetTokenRepository.GetByTokenHashAsync(HashToken(request.Token), cancellationToken);
        if (resetToken == null || resetToken.UsedAt != null || resetToken.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("O link de redefinição é inválido ou expirou.");

        var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("O link de redefinição é inválido ou expirou.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        await _userRepository.UpdatePasswordHashAsync(user.Id, passwordHash, cancellationToken);
        await _passwordResetTokenRepository.MarkUsedAsync(resetToken.Id, cancellationToken);
    }

    private string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new("type", user.Type.ToString())
        };

        if (user.ChildId != null)
            claims.Add(new Claim("childId", user.ChildId));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateResetToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static AuthUserResponse MapToResponse(User u) =>
        new(u.Id, u.Name, u.Email, u.Type.ToString(), u.ChildId);
}

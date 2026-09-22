using KidsRewards.Api.DTOs;

namespace KidsRewards.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthUserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthLoginResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthUserResponse?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
}

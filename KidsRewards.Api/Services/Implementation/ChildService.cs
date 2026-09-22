using KidsRewards.Api.Authentication;
using KidsRewards.Api.Domain.Entities;
using KidsRewards.Api.Domain.Enums;
using KidsRewards.Api.DTOs;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Services.Interfaces;

namespace KidsRewards.Api.Services.Implementation;

public class ChildService : IChildService
{
    private readonly IChildRepository _childRepository;
    private readonly IUserRepository _userRepository;

    public ChildService(IChildRepository childRepository, IUserRepository userRepository)
    {
        _childRepository = childRepository;
        _userRepository = userRepository;
    }

    public async Task<ChildResponse> CreateAsync(UserContext context, CreateChildRequest request, CancellationToken cancellationToken = default)
    {
        var child = new Child
        {
            Name = request.Name.Trim(),
            Age = request.Age,
            ResponsibleUserId = context.UserId,
            PointsBalance = 0,
            MonthlyGoalPoints = request.MonthlyGoalPoints,
            MonthlyGoalRewardId = request.MonthlyGoalRewardId,
            CreatedAt = DateTime.UtcNow
        };

        await _childRepository.CreateAsync(child, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.LoginEmail) || !string.IsNullOrWhiteSpace(request.LoginPassword))
        {
            if (string.IsNullOrWhiteSpace(request.LoginEmail) || string.IsNullOrWhiteSpace(request.LoginPassword))
                throw new InvalidOperationException("Para criar o acesso da criança, informe email e senha.");

            await CreateChildUserAsync(child, request.LoginEmail, request.LoginPassword, cancellationToken);
        }

        return MapToResponse(child);
    }

    public async Task<IEnumerable<ChildResponse>> GetForUserAsync(UserContext context, CancellationToken cancellationToken = default)
    {
        IEnumerable<Child> children;

        if (context.IsChild)
        {
            var own = context.ChildId == null ? null : await _childRepository.GetByIdAsync(context.ChildId, cancellationToken);
            children = own == null ? Array.Empty<Child>() : new[] { own };
        }
        else
        {
            children = await _childRepository.GetByResponsibleIdAsync(context.UserId, cancellationToken);
        }

        return children.Select(MapToResponse);
    }

    public async Task<ChildResponse?> GetByIdAsync(string id, UserContext context, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(id, cancellationToken);
        if (child == null || !CanAccess(child, context)) return null;
        return MapToResponse(child);
    }

    public async Task<bool> UpdateGoalAsync(string childId, UserContext context, UpdateChildGoalRequest request, CancellationToken cancellationToken = default)
    {
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child == null || !CanAccess(child, context)) return false;

        return await _childRepository.UpdateGoalAsync(childId, request.MonthlyGoalPoints, request.MonthlyGoalRewardId, cancellationToken);
    }

    private async Task CreateChildUserAsync(Child child, string email, string password, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException("Já existe uma conta com este email de acesso.");

        var user = new User
        {
            Name = child.Name,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Type = UserType.Child,
            ChildId = child.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, cancellationToken);
    }

    private static bool CanAccess(Child child, UserContext context)
    {
        if (context.IsChild) return context.ChildId == child.Id;
        return child.ResponsibleUserId == context.UserId;
    }

    private static ChildResponse MapToResponse(Child c) =>
        new(c.Id, c.Name, c.Age, c.PointsBalance, c.MonthlyGoalPoints, c.MonthlyGoalRewardId, c.CreatedAt);
}

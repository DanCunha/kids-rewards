using System.ComponentModel.DataAnnotations;

namespace KidsRewards.Api.DTOs;

public record CreateChildRequest(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    string Name,

    [Range(1, 18, ErrorMessage = "A idade deve estar entre 1 e 18 anos.")]
    int Age,

    [Range(0, 100000, ErrorMessage = "A meta de pontos não pode ser negativa.")]
    int MonthlyGoalPoints = 0,

    string? MonthlyGoalRewardId = null
);

public record UpdateChildGoalRequest(
    [Range(0, 100000, ErrorMessage = "A meta de pontos não pode ser negativa.")]
    int MonthlyGoalPoints,

    string? MonthlyGoalRewardId
);

public record ChildResponse(
    string Id,
    string Name,
    int Age,
    int PointsBalance,
    int MonthlyGoalPoints,
    string? MonthlyGoalRewardId,
    DateTime CreatedAt
);
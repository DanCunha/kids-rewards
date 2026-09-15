using System.ComponentModel.DataAnnotations;

namespace KidsRewards.Api.DTOs;

public record CreateRewardRequest(
    [Required(ErrorMessage = "O nome da recompensa é obrigatório.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 150 caracteres.")]
    string Name,

    [Range(1, 100000, ErrorMessage = "Os pontos necessários devem ser maiores que zero.")]
    int RequiredPoints
);

public record RedeemRewardRequest(
    [Required(ErrorMessage = "O ID da criança é obrigatório.")]
    string ChildId,

    [Required(ErrorMessage = "O ID da recompensa é obrigatório.")]
    string RewardId
);

public record UpdateRewardActiveRequest(
    [Required(ErrorMessage = "O status ativo é obrigatório.")]
    bool IsActive
);

public record RewardResponse(
    string Id,
    string Name,
    int RequiredPoints,
    bool IsActive,
    DateTime CreatedAt
);
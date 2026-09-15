using System.ComponentModel.DataAnnotations;

namespace KidsRewards.Api.DTOs;

public record CreateActivityRequest(
    [Required(ErrorMessage = "O ID da criança é obrigatório.")]
    string ChildId,

    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 150 caracteres.")]
    string Title,

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
    string Description,

    [Range(1, 10000, ErrorMessage = "A pontuação da atividade deve ser maior que zero.")]
    int Points
);

public record ActivityResponse(
    string Id,
    string ChildId,
    string Title,
    string Description,
    int Points,
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime CreatedAt
);
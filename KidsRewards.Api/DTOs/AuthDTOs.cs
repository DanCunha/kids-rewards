using System.ComponentModel.DataAnnotations;

namespace KidsRewards.Api.DTOs;

public record RegisterRequest(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    string Name,

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    [StringLength(200, ErrorMessage = "O email não pode exceder 200 caracteres.")]
    string Email,

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres.")]
    string Password
);

public record LoginRequest(
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    string Email,

    [Required(ErrorMessage = "A senha é obrigatória.")]
    string Password
);

public record AuthUserResponse(
    string Id,
    string Name,
    string Email,
    string Type,
    string? ChildId
);

public record AuthLoginResult(string Token, AuthUserResponse User);

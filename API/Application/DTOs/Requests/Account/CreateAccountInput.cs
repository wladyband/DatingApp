using System.ComponentModel.DataAnnotations;

namespace API.Application.DTOs.Requests.Account;

/// <summary>
/// DTO de entrada para criação de conta.
/// </summary>
public record CreateAccountInput(
    [param: Required(ErrorMessage = "O e-mail é obrigatório.")]
    [param: EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    string Email,

    [param: Required(ErrorMessage = "O nome de exibição é obrigatório.")]
    [param: StringLength(50, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 50 caracteres.")]
    string Displayname,

    [param: Required(ErrorMessage = "A senha é obrigatória.")]
    [param: MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    string Password
);

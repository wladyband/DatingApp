namespace API.Application.DTOs.Requests.Account;

/// <summary>
/// DTO de entrada para criação de conta.
/// </summary>
public record CreateAccountInput(
    string Email,
    string Displayname,
    string Password
);

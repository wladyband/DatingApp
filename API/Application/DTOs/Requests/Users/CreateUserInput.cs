namespace API.Application.DTOs.Requests.Users;

/// <summary>
/// DTO de entrada para criação de usuário.
/// </summary>
public record CreateUserInput(string Email, string Displayname, string Password);

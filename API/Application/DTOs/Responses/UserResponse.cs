namespace API.Application.DTOs.Responses;

/// <summary>
/// DTO de resposta para usuário. Expõe apenas o necessário para o cliente.
/// Permite evolução da entidade sem quebrar a API.
/// </summary>
public record UserResponse(
    string Id,
    string Email,
    string Displayname
);

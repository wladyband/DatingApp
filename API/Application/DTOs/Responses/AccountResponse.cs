namespace API.Application.DTOs.Responses;

/// <summary>
/// DTO de resposta para conta. Expõe apenas o necessário para o cliente.
/// </summary>
public record AccountResponse(
    string Id,
    string Email,
    string Displayname
);

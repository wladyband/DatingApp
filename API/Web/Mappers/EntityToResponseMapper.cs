using API.Domain.Entities;
using API.Web.Responses;

namespace API.Web.Mappers;

/// <summary>
/// Mapeador de Entities para DTOs de resposta.
/// Centraliza a lógica de transformação entre camadas.
/// </summary>
public static class EntityToResponseMapper
{
    public static UserResponse ToUserResponse(this AppUser user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            Displayname: user.Displayname
        );

    public static AccountResponse ToAccountResponse(this AppUser user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            Displayname: user.Displayname
        );

    public static IEnumerable<UserResponse> ToUserResponseList(this IEnumerable<AppUser> users) =>
        users.Select(u => u.ToUserResponse());
}

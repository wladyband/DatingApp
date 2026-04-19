using API.Application.DTOs.Responses;
using API.Core.Entities;

namespace API.Application.Services;

/// <summary>
/// Mapeador de Entities para DTOs de resposta.
/// Centraliza a lógica de transformação entre camadas.
/// </summary>
public static class EntityToDtoMapper
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

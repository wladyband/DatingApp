using API.Domain.Entities;
using API.Application.DTOs.Requests.Users;
using API.Web.Responses;

namespace API.Web.Mappers;

/// <summary>
/// Mapeador de entidades/DTOs para DTOs de resposta.
/// Centraliza a lógica de transformação entre camadas.
/// </summary>
public static class EntityToResponseMapper
{
    public static UserResponse ToUserResponse(this AppUser user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName
        );

    public static UserResponse ToUserResponse(this UserInput user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName
        );

    public static AccountResponse ToAccountResponse(this AppUser user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            Token: null
        );

    public static AccountResponse ToAccountResponse(this UserInput user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            Token: user.Token
        );

    public static IEnumerable<UserResponse> ToUserResponseList(this IEnumerable<AppUser> users) =>
        users.Select(u => u.ToUserResponse());
}

using API.Application.Services;
using API.Application.UseCases.Account;
using API.Application.UseCases.Users;

namespace API.Application;

/// <summary>
/// Extension para registrar todos os Use Cases e Application Services no container DI.
/// Facilita o gerenciamento centralizado de dependências da Application.
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Use Cases
        services.AddScoped<CreateUserUseCase>();
        services.AddScoped<GetUserByIdUseCase>();
        services.AddScoped<GetAllUsersUseCase>();
        services.AddScoped<DeleteUserUseCase>();
        services.AddScoped<CreateAccountUseCase>();

        // Application Services (orquestração de use cases)
        services.AddScoped<UserApplicationService>();
        services.AddScoped<AccountApplicationService>();

        return services;
    }
}

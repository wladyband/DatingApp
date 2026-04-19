using API.Application.UseCases.Account;
using API.Application.UseCases.Users;

namespace API.Application;

/// <summary>
/// Extension para registrar os use cases da camada Application no container DI.
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

        return services;
    }
}

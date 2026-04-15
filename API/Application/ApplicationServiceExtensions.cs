using API.Application.UseCases.Users;

namespace API.Application;

/// <summary>
/// Extension para registrar todos os Use Cases no container DI.
/// Facilita o gerenciamento centralizado de dependências da Application.
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registrar use cases
        services.AddScoped<CreateUserUseCase>();
        services.AddScoped<GetUserByIdUseCase>();

        return services;
    }
}

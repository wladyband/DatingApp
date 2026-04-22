using API.Infrastructure.Persistence.DependencyInjection;
using API.Infrastructure.PostgreSql.DependencyInjection;

namespace API.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddPersistenceInitializationServices()
            .AddPersistenceOptions(configuration)
            .AddOptionalPostgreSqlInfrastructure(configuration)
            .AddConfiguredPersistenceProvider(configuration);
    }
}

namespace API.Infrastructure.PostgreSql.DependencyInjection;

public static class PostgreSqlInfrastructureRegistrationExtensions
{
    public static IServiceCollection AddOptionalPostgreSqlInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registra NpgsqlDataSource sempre que houver connection string,
        // permitindo aplicar migrations independente do provider ativo.
        var postgresConnection = configuration.GetConnectionString("PostgreSql");
        if (!string.IsNullOrWhiteSpace(postgresConnection))
        {
            services.AddPostgreSqlServices(configuration);
        }

        return services;
    }
}
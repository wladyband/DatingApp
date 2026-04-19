using Npgsql;
using API.Infrastructure.PostgreSql.Subscriptions;

namespace API.Infrastructure.PostgreSql;

public static class PostgreSqlServiceExtensions
{
    public static IServiceCollection AddPostgreSqlServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddPostgreSqlCore(configuration)
            .AddPostgreSqlRepositories();
    }

    public static IServiceCollection AddPostgreSqlCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgresConnection = configuration.GetConnectionString("PostgreSql");

        if (string.IsNullOrWhiteSpace(postgresConnection))
            throw new InvalidOperationException("ConnectionStrings:PostgreSql não foi configurada.");

        services.AddSingleton(_ => NpgsqlDataSource.Create(postgresConnection));

        return services;
    }

    public static IServiceCollection AddPostgreSqlRepositories(this IServiceCollection services)
    {
        return services.AddPostgreSqlSubscriptionsModule();
    }
}

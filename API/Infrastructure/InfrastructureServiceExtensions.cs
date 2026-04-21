using API.Infrastructure.MongoDb;
using API.Infrastructure.MongoDb.Configuration;
using API.Infrastructure.PostgreSql;
using API.Infrastructure.PostgreSql.Configuration;
using SeedDataOptions = API.Infrastructure.MongoDb.Configuration.SeedDataOptions;

namespace API.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register persistence initialization service
        services.AddScoped<IPostgreSqlInitializer, PostgreSqlInitializer>();
        services.AddScoped<IMongoDbInitializer, MongoDbInitializer>();
        services.AddScoped<IPersistenceInitializationService, PersistenceInitializationService>();

        var persistenceOptions = configuration
            .GetSection(PersistenceOptions.SectionName)
            .Get<PersistenceOptions>() ?? new PersistenceOptions();

        services.Configure<PersistenceOptions>(
            configuration.GetSection(PersistenceOptions.SectionName));

        services.Configure<MongoDbOptions>(
            configuration.GetSection(MongoDbOptions.SectionName));

        services.Configure<SeedDataOptions>(
            configuration.GetSection(SeedDataOptions.SectionName));

        // PostgreSQL: registra NpgsqlDataSource sempre que a connection string estiver
        // configurada, para que o runner de migrations funcione independente do provider ativo.
        var postgresConnection = configuration.GetConnectionString("PostgreSql");
        if (!string.IsNullOrWhiteSpace(postgresConnection))
        {
            services.AddPostgreSqlServices(configuration);
        }

        if (string.Equals(persistenceOptions.Provider, "MongoDb", StringComparison.OrdinalIgnoreCase))
        {
            services.AddMongoDbServices(configuration);
        }
        else
        {
            throw new InvalidOperationException(
                $"Persistence:Provider '{persistenceOptions.Provider}' não é suportado. Use 'MongoDb'.");
        }

        return services;
    }
}

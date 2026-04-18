using API.Application.Ports;
using API.Core.Entities;
using API.Infrastructure.Configuration;
using MongoDB.Bson.Serialization;
using API.Infrastructure.Persistence;
using MongoDB.Driver;
using Npgsql;

namespace API.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var persistenceOptions = configuration
            .GetSection(PersistenceOptions.SectionName)
            .Get<PersistenceOptions>() ?? new PersistenceOptions();

        services.Configure<PersistenceOptions>(
            configuration.GetSection(PersistenceOptions.SectionName));

        services.Configure<MongoDbOptions>(
            configuration.GetSection(MongoDbOptions.SectionName));

        services.Configure<SeedDataOptions>(
            configuration.GetSection(SeedDataOptions.SectionName));

        // PostgreSQL é sempre registrado quando a connection string estiver configurada,
        // independente do provider ativo. Isso permite que o runner de migrations rode
        // mesmo quando o MongoDB é o provider principal (ex: para futura área de pagamentos).
        var postgresConnection = configuration.GetConnectionString("PostgreSql");
        if (!string.IsNullOrWhiteSpace(postgresConnection))
        {
            services.AddSingleton(_ => NpgsqlDataSource.Create(postgresConnection));
        }

        if (string.Equals(persistenceOptions.Provider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(postgresConnection))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:PostgreSql não foi configurada.");
            }

            services.AddScoped<PostgreSqlUserRepository>();

            services.AddScoped<IUserRepository>(sp =>
                sp.GetRequiredService<PostgreSqlUserRepository>());
        }
        else if (string.Equals(persistenceOptions.Provider, "MongoDb", StringComparison.OrdinalIgnoreCase))
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(AppUser)))
            {
                BsonClassMap.RegisterClassMap<AppUser>(classMap =>
                {
                    classMap.AutoMap();
                    classMap.SetIgnoreExtraElements(true);
                });
            }

            var mongoOptions = configuration
                .GetSection(MongoDbOptions.SectionName)
                .Get<MongoDbOptions>() ?? new MongoDbOptions();

            if (string.IsNullOrWhiteSpace(mongoOptions.ConnectionString))
            {
                throw new InvalidOperationException(
                    "MongoDb:ConnectionString não foi configurada.");
            }

            if (string.IsNullOrWhiteSpace(mongoOptions.Database))
            {
                throw new InvalidOperationException(
                    "MongoDb:Database não foi configurada.");
            }

            services.AddSingleton<IMongoClient>(_ =>
                new MongoClient(mongoOptions.ConnectionString));

            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoOptions.Database);
            });

            services.AddScoped<MongoUserRepository>();

            services.AddScoped<IUserRepository>(sp =>
                sp.GetRequiredService<MongoUserRepository>());
        }
        else
        {
            throw new InvalidOperationException(
                $"Persistence:Provider '{persistenceOptions.Provider}' não é suportado. Use 'PostgreSql' ou 'MongoDb'.");
        }

        return services;
    }
}

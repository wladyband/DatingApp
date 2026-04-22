using API.Infrastructure.MongoDb.DependencyInjection;
using API.Infrastructure.MongoDb.Configuration;
using API.Infrastructure.PostgreSql.Configuration;
using SeedDataOptions = API.Infrastructure.MongoDb.Configuration.SeedDataOptions;

namespace API.Infrastructure.Persistence.DependencyInjection;

public static class PersistenceInfrastructureRegistrationExtensions
{
    public static IServiceCollection AddPersistenceInitializationServices(this IServiceCollection services)
    {
        services.AddScoped<IPostgreSqlInitializer, PostgreSqlInitializer>();
        services.AddScoped<IMongoDbInitializer, MongoDbInitializer>();
        services.AddScoped<IPersistenceInitializationService, PersistenceInitializationService>();

        return services;
    }

    public static IServiceCollection AddPersistenceOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PersistenceOptions>(
            configuration.GetSection(PersistenceOptions.SectionName));

        services.Configure<MongoDbOptions>(
            configuration.GetSection(MongoDbOptions.SectionName));

        services.Configure<SeedDataOptions>(
            configuration.GetSection(SeedDataOptions.SectionName));

        return services;
    }

    public static IServiceCollection AddConfiguredPersistenceProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var persistenceOptions = configuration
            .GetSection(PersistenceOptions.SectionName)
            .Get<PersistenceOptions>() ?? new PersistenceOptions();

        if (string.Equals(persistenceOptions.Provider, "MongoDb", StringComparison.OrdinalIgnoreCase))
        {
            services.AddRequiredMongoDbInfrastructure(configuration);
            return services;
        }

        throw new InvalidOperationException(
            $"Persistence:Provider '{persistenceOptions.Provider}' não é suportado. Use 'MongoDb'.");
    }
}
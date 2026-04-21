using API.Infrastructure.MongoDb;
using API.Infrastructure.PostgreSql;
using API.Infrastructure.PostgreSql.Configuration;
using Microsoft.Extensions.Logging;

namespace API.Infrastructure;

/// <summary>
/// Service responsible for initializing persistence layer (PostgreSQL and MongoDB).
/// Extracted from the extension method to enable unit testing.
/// </summary>
public interface IPersistenceInitializationService
{
    /// <summary>
    /// Initializes the persistence layer based on configuration.
    /// </summary>
    Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger);
}

public interface IPostgreSqlInitializer
{
    Task InitializeIfEnabledAsync(IServiceProvider services, IConfiguration configuration, ILogger logger);
}

public interface IMongoDbInitializer
{
    Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger);
}

public sealed class PostgreSqlInitializer : IPostgreSqlInitializer
{
    public Task InitializeIfEnabledAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        return services.InitializePostgreSqlIfEnabledAsync(configuration, logger);
    }
}

public sealed class MongoDbInitializer : IMongoDbInitializer
{
    public Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        return services.InitializeMongoDbAsync(configuration, logger);
    }
}

/// <summary>
/// Implementation of persistence initialization service.
/// Handles the logic for initializing PostgreSQL and MongoDB based on PersistenceOptions.
/// </summary>
public sealed class PersistenceInitializationService : IPersistenceInitializationService
{
    private readonly IPostgreSqlInitializer _postgreSqlInitializer;
    private readonly IMongoDbInitializer _mongoDbInitializer;

    public PersistenceInitializationService(
        IPostgreSqlInitializer postgreSqlInitializer,
        IMongoDbInitializer mongoDbInitializer)
    {
        _postgreSqlInitializer = postgreSqlInitializer;
        _mongoDbInitializer = mongoDbInitializer;
    }

    public async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        var persistenceOptions = GetPersistenceOptions(configuration);

        // PostgreSQL: run migrations only when explicitly enabled in configuration.
        // This prevents accidental changes to the PostgreSQL database while the
        // subscription domain is not yet in development.
        logger.LogInformation("Initializing PostgreSQL if enabled...");
        await _postgreSqlInitializer.InitializeIfEnabledAsync(services, configuration, logger);

        // Seed data belongs exclusively to MongoDB (users and exercises).
        // PostgreSQL does not receive seed at this moment.
        if (!IsMongoDbProvider(persistenceOptions))
        {
            logger.LogInformation("Provider is not MongoDB. Skipping MongoDB initialization.");
            return;
        }

        logger.LogInformation("Initializing MongoDB...");
        await _mongoDbInitializer.InitializeAsync(services, configuration, logger);
    }

    private static PersistenceOptions GetPersistenceOptions(IConfiguration configuration)
    {
        var persistenceOptions = configuration
            .GetSection(PersistenceOptions.SectionName)
            .Get<PersistenceOptions>();

        return persistenceOptions ?? new PersistenceOptions();
    }

    private static bool IsMongoDbProvider(PersistenceOptions options)
    {
        return string.Equals(options.Provider, "MongoDb", StringComparison.OrdinalIgnoreCase);
    }
}

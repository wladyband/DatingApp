namespace API.Infrastructure;

/// <summary>
/// Extension methods for infrastructure initialization.
/// Responsibility: Bridge between WebApplication startup and persistence initialization service.
/// </summary>
public static class InfrastructureInitializationExtensions
{
    /// <summary>
    /// Initializes persistence layer (PostgreSQL and MongoDB) asynchronously.
    /// Creates a service scope and delegates to IPersistenceInitializationService.
    /// </summary>
    public static async Task InitializePersistenceAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var configuration = services.GetRequiredService<IConfiguration>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("PersistenceInitialization");

        var persistenceService = services.GetRequiredService<IPersistenceInitializationService>();
        await persistenceService.InitializeAsync(services, configuration, logger);
    }
}
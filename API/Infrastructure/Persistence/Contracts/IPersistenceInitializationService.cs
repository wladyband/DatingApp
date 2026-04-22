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
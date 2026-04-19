using API.Infrastructure.Configuration;
using API.Infrastructure.MongoDb.Accounts;
using API.Infrastructure.MongoDb.Users;

namespace API.Infrastructure.MongoDb;

public static class MongoDbInitializationExtensions
{
    public static async Task InitializeMongoDbAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        var seedData = configuration
            .GetSection(SeedDataOptions.SectionName)
            .Get<SeedDataOptions>() ?? new SeedDataOptions();

        if (!seedData.Enabled)
        {
            logger.LogInformation("SeedData está desabilitado por configuração.");
            return;
        }

        if (!IsValidSeedMode(seedData.Mode))
        {
            throw new InvalidOperationException(
                $"SeedData:Mode '{seedData.Mode}' é inválido. Use '{SeedDataModes.Upsert}' ou '{SeedDataModes.IfEmpty}'.");
        }

        await services.InitializeMongoDbUsersAsync(configuration, seedData, logger);
        await services.InitializeMongoDbAccountsAsync(configuration, logger);
    }

    private static bool IsValidSeedMode(string mode)
    {
        var normalizedMode = mode?.Trim();

        return string.Equals(normalizedMode, SeedDataModes.Upsert, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedMode, SeedDataModes.IfEmpty, StringComparison.OrdinalIgnoreCase);
    }
}

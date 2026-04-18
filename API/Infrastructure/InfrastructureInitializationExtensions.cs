using API.Core.Entities;
using API.Infrastructure.Configuration;
using API.Infrastructure.Persistence;
using MongoDB.Driver;
using Npgsql;

namespace API.Infrastructure;

public static class InfrastructureInitializationExtensions
{
    public static async Task InitializePersistenceAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("PersistenceInitialization");

        var persistenceOptions = configuration
            .GetSection(PersistenceOptions.SectionName)
            .Get<PersistenceOptions>() ?? new PersistenceOptions();

        // PostgreSQL: aplica migrations de versionamento sempre que a connection string
        // estiver configurada, independente do provider ativo. Não popula dados aqui —
        // o domínio de pagamentos (futuro) cuidará do próprio seed quando suas entidades
        // forem criadas.
        await ApplyPostgreSqlMigrationsIfConfiguredAsync(services, logger);

        // Seed de dados pertence exclusivamente ao MongoDB (usuários e exercícios).
        // PostgreSQL não recebe seed neste momento.
        if (!string.Equals(persistenceOptions.Provider, "MongoDb", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

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

        await InitializeMongoDbAsync(services, configuration, seedData, logger);
    }

    private static async Task ApplyPostgreSqlMigrationsIfConfiguredAsync(
        IServiceProvider services,
        ILogger logger)
    {
        var dataSource = services.GetService<NpgsqlDataSource>();
        if (dataSource is null)
        {
            logger.LogDebug("PostgreSQL não configurado. Migrações não serão aplicadas.");
            return;
        }

        var environment = services.GetRequiredService<IWebHostEnvironment>();
        await PostgreSqlMigrationRunner.ApplyPendingMigrationsAsync(dataSource, environment, logger);
    }

    private static async Task InitializeMongoDbAsync(
        IServiceProvider services,
        IConfiguration configuration,
        SeedDataOptions seedData,
        ILogger logger)
    {
        var database = services.GetRequiredService<IMongoDatabase>();
        var mongoOptions = configuration
            .GetSection(MongoDbOptions.SectionName)
            .Get<MongoDbOptions>() ?? new MongoDbOptions();

        var collectionName = mongoOptions.UsersCollection;
        var collection = database.GetCollection<AppUser>(collectionName);

        var existingCollections = await (await database.ListCollectionNamesAsync())
            .ToListAsync();

        if (!existingCollections.Contains(collectionName))
        {
            await database.CreateCollectionAsync(collectionName);
        }

        var emailIndex = new CreateIndexModel<AppUser>(
            Builders<AppUser>.IndexKeys.Ascending(user => user.Email),
            new CreateIndexOptions { Unique = true, Name = "IX_Users_Email" });

        await collection.Indexes.CreateOneAsync(emailIndex);

        if (string.Equals(seedData.Mode, SeedDataModes.IfEmpty, StringComparison.OrdinalIgnoreCase))
        {
            var usersCount = await collection.CountDocumentsAsync(Builders<AppUser>.Filter.Empty);
            if (usersCount > 0)
            {
                logger.LogInformation("SeedData em modo IfEmpty: MongoDB já possui dados, seed ignorado.");
                return;
            }
        }

        var users = BuildSeedUsers(seedData).ToList();
        if (users.Count == 0)
        {
            logger.LogInformation("Nenhum usuário de seed configurado para MongoDB.");
            return;
        }

        foreach (var user in users)
        {
            var filter = Builders<AppUser>.Filter.Eq(existing => existing.Email, user.Email);
            var update = Builders<AppUser>.Update
                .Set(existing => existing.Displayname, user.Displayname)
                .SetOnInsert(existing => existing.Id, user.Id)
                .SetOnInsert(existing => existing.Email, user.Email);

            await collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        }

        logger.LogInformation("Seed sincronizado no MongoDB com {Count} usuário(s).", users.Count);
    }

    private static IEnumerable<AppUser> BuildSeedUsers(SeedDataOptions seedData)
    {
        return seedData.Users
            .Where(user =>
                !string.IsNullOrWhiteSpace(user.Email) &&
                !string.IsNullOrWhiteSpace(user.Displayname))
            .Select(user => new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                Displayname = user.Displayname.Trim(),
                Email = user.Email.Trim()
            });
    }

    private static bool IsValidSeedMode(string mode)
    {
        var normalizedMode = mode?.Trim();

        return string.Equals(normalizedMode, SeedDataModes.Upsert, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedMode, SeedDataModes.IfEmpty, StringComparison.OrdinalIgnoreCase);
    }
}
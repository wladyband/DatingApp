using API.Domain.Entities;
using API.Infrastructure.MongoDb.Configuration;
using MongoDB.Driver;

namespace API.Infrastructure.MongoDb.Users;

public static class MongoDbUsersInitializationExtensions
{
    public static async Task InitializeMongoDbUsersAsync(
        this IServiceProvider services,
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
                .Set(existing => existing.DisplayName, user.DisplayName)
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
                !string.IsNullOrWhiteSpace(user.DisplayName))
            .Select(user => new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = user.DisplayName.Trim(),
                Email = user.Email.Trim(),
                PasswordHash = Array.Empty<byte>(),
                PasswordSalt = Array.Empty<byte>()
            });
    }
}

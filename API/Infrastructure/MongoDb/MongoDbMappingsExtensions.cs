using API.Domain.Entities;
using MongoDB.Bson.Serialization;

namespace API.Infrastructure.MongoDb;

public static class MongoDbMappingsExtensions
{
    public static IServiceCollection RegisterMongoDbMappings(this IServiceCollection services)
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(AppUser)))
        {
            BsonClassMap.RegisterClassMap<AppUser>(classMap =>
            {
                classMap.AutoMap();
                classMap.SetIgnoreExtraElements(true);
            });
        }

        return services;
    }
}

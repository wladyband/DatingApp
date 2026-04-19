namespace API.Infrastructure.MongoDb.Configuration;

public class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string UsersCollection { get; set; } = "users";
}

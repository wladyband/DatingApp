namespace API.Infrastructure.Configuration;

public class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string Provider { get; set; } = "PostgreSql";
}

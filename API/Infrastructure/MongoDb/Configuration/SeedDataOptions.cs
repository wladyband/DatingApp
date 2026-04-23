namespace API.Infrastructure.MongoDb.Configuration;

/// <summary>
/// Configurações para seed de dados em MongoDB.
/// Específico do adapter MongoDB.
/// </summary>
public class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public bool Enabled { get; set; } = true;
    public string Mode { get; set; } = SeedDataModes.Upsert;
    public List<SeedUserOptions> Users { get; set; } = [];
}

public static class SeedDataModes
{
    public const string Upsert = "Upsert";
    public const string IfEmpty = "IfEmpty";
}

public class SeedUserOptions
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

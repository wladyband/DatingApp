namespace API.Infrastructure.Configuration;

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
    public string Displayname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
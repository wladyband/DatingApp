using API.Infrastructure;
using API.Infrastructure.PostgreSql.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace API.UnitTests.Infrastructure;

public class InfrastructureServiceExtensionsTests
{
    [Fact]
    public void AddInfrastructureServices_Should_Register_IPostgreSqlInitializer_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration("MongoDb");

        // Act
        services.AddInfrastructureServices(config);
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPostgreSqlInitializer));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructureServices_Should_Register_IMongoDbInitializer_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration("MongoDb");

        // Act
        services.AddInfrastructureServices(config);
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoDbInitializer));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructureServices_Should_Register_IPersistenceInitializationService_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration("MongoDb");

        // Act
        services.AddInfrastructureServices(config);
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPersistenceInitializationService));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructureServices_Should_Configure_PersistenceOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration("MongoDb");

        // Act
        services.AddInfrastructureServices(config);
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PersistenceOptions>>();

        // Assert
        options.Value.Provider.Should().Be("MongoDb");
    }

    [Fact]
    public void AddInfrastructureServices_Should_Return_Same_IServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration("MongoDb");

        // Act
        var result = services.AddInfrastructureServices(config);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInfrastructureServices_Should_Throw_When_Provider_Is_Not_MongoDb()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration("PostgreSQL");

        // Act
        var act = () => services.AddInfrastructureServices(config);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*PostgreSQL*Use 'MongoDb'*");
    }

    [Theory]
    [InlineData("MongoDb")]
    [InlineData("mongodb")]
    [InlineData("MONGODB")]
    public void AddInfrastructureServices_Should_Accept_MongoDb_Provider_Case_Insensitive(string provider)
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration(provider);

        // Act
        var act = () => services.AddInfrastructureServices(config);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddInfrastructureServices_Should_Not_Add_PostgreSql_Services_When_Connection_String_Is_Missing()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration("MongoDb", postgresConnection: null);
        var initialCount = services.Count;

        // Act
        services.AddInfrastructureServices(config);

        // Assert - AddPostgreSqlServices was NOT called because no connection string was found
        // Verify by checking that Npgsql-specific types were NOT registered
        var npgsqlDescriptor = services.FirstOrDefault(d =>
            d.ServiceType.FullName?.Contains("Npgsql") == true);
        npgsqlDescriptor.Should().BeNull();
    }

    [Fact]
    public void AddInfrastructureServices_Should_Use_Default_Provider_When_Section_Is_Missing()
    {
        // Arrange
        var services = new ServiceCollection();
        // No Persistence section: default Provider = "MongoDb", so MongoDb ConnectionString is required
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = "mongodb://localhost:27017",
                ["MongoDb:Database"] = "DatingApp"
            })
            .Build();

        // Act - Default Provider is "MongoDb" so this should not throw
        var act = () => services.AddInfrastructureServices(config);

        // Assert
        act.Should().NotThrow();
    }

    private static IConfiguration CreateConfiguration(string provider, string? postgresConnection = null)
    {
        var data = new Dictionary<string, string?>
        {
            ["Persistence:Provider"] = provider,
            // Required by AddMongoDbCore when provider is MongoDb
            ["MongoDb:ConnectionString"] = "mongodb://localhost:27017",
            ["MongoDb:Database"] = "DatingApp"
        };

        if (postgresConnection != null)
            data["ConnectionStrings:PostgreSql"] = postgresConnection;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
    }
}

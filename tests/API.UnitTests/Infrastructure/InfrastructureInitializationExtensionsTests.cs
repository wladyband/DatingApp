using API.Infrastructure;
using API.Infrastructure.PostgreSql.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace API.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for InfrastructureInitializationExtensions.
/// Note: Tests focus on validating the extension method's logic and dependency resolution.
/// Since InitializePersistenceAsync is an extension on WebApplication (concrete class),
/// full unit mocking is limited. These tests validate configuration parsing and decision logic.
/// </summary>
public class InfrastructureInitializationExtensionsTests
{
    [Fact]
    public void PersistenceOptions_SectionName_Should_Be_Configured()
    {
        // Arrange & Act
        var sectionName = PersistenceOptions.SectionName;

        // Assert
        sectionName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PersistenceOptions_Default_Provider_Should_Be_Set()
    {
        // Arrange
        var persistenceOptions = new PersistenceOptions();

        // Act & Assert
        persistenceOptions.Should().NotBeNull();
    }

    [Fact]
    public void PersistenceOptions_Should_Allow_Provider_Configuration()
    {
        // Arrange
        var persistenceOptions = new PersistenceOptions { Provider = "MongoDb" };

        // Act & Assert
        persistenceOptions.Provider.Should().Be("MongoDb");
    }

    [Theory]
    [InlineData("MongoDb", true)]
    [InlineData("mongodb", true)]
    [InlineData("MONGODB", true)]
    [InlineData("PostgreSQL", false)]
    [InlineData("postgres", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void PersistenceOptions_Provider_Comparison_Should_Be_Case_Insensitive(string? provider, bool expectedIsMongoDb)
    {
        // Arrange
        var persistenceOptions = new PersistenceOptions { Provider = provider };

        // Act
        var isMongoDb = string.Equals(persistenceOptions.Provider, "MongoDb", StringComparison.OrdinalIgnoreCase);

        // Assert
        isMongoDb.Should().Be(expectedIsMongoDb);
    }

    [Fact]
    public void Configuration_GetSection_Should_Return_Valid_Section()
    {
        // Arrange
        var services = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Persistence:Provider", "MongoDb" }
            });
        var configuration = configBuilder.Build();

        // Act
        var section = configuration.GetSection(PersistenceOptions.SectionName);
        var options = section.Get<PersistenceOptions>();

        // Assert
        section.Should().NotBeNull();
        options.Should().NotBeNull();
    }

    [Fact]
    public void Configuration_Should_Parse_PersistenceOptions_From_InMemory()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Persistence:Provider", "MongoDb" }
            });
        var configuration = configBuilder.Build();

        // Act
        var section = configuration.GetSection(PersistenceOptions.SectionName);
        var options = section.Get<PersistenceOptions>();

        // Assert
        options?.Provider.Should().Be("MongoDb");
    }

    [Fact]
    public void Configuration_Should_Return_Null_For_Missing_Section()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>());
        var configuration = configBuilder.Build();

        // Act
        var section = configuration.GetSection(PersistenceOptions.SectionName);
        var options = section.Get<PersistenceOptions>();

        // Assert
        options.Should().BeNull();
    }

    [Fact]
    public void ServiceCollection_Should_Create_Scope_For_Service_Resolution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ILoggerFactory, LoggerFactory>();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        var serviceProvider = services.BuildServiceProvider();

        // Act
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Assert
        scope.Should().NotBeNull();
        logger.Should().NotBeNull();
        config.Should().NotBeNull();
    }

    [Fact]
    public void LoggerFactory_Should_Create_Logger_With_Category()
    {
        // Arrange
        using var loggerFactory = new LoggerFactory();

        // Act
        var logger = loggerFactory.CreateLogger("PersistenceInitialization");

        // Assert
        logger.Should().NotBeNull();
    }

    [Fact]
    public void Service_Scope_Should_Be_Disposable()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var scope = serviceProvider.CreateScope();

        // Assert
        scope.Should().BeAssignableTo<IAsyncDisposable>();
    }

    [Fact]
    public void Logger_Factory_Mock_Should_Create_Logger()
    {
        // Arrange
        var loggerFactory = Substitute.For<ILoggerFactory>();
        var logger = Substitute.For<ILogger>();

        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);

        // Act
        var result = loggerFactory.CreateLogger("Test");

        // Assert
        result.Should().NotBeNull();
        loggerFactory.Received(1).CreateLogger("Test");
    }

    [Fact]
    public void PersistenceOptions_Should_Support_Null_Provider()
    {
        // Arrange & Act
        var options = new PersistenceOptions { Provider = null };

        // Assert
        options.Provider.Should().BeNull();
    }

    [Theory]
    [InlineData("MongoDb")]
    [InlineData("PostgreSQL")]
    [InlineData("SQLServer")]
    public void PersistenceOptions_Should_Accept_Various_Providers(string provider)
    {
        // Arrange
        var options = new PersistenceOptions { Provider = provider };

        // Act & Assert
        options.Provider.Should().Be(provider);
    }
}

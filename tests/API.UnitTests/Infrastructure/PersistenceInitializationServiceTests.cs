using API.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace API.UnitTests.Infrastructure;

public class PersistenceInitializationServiceTests
{
    private readonly IPostgreSqlInitializer _postgreSqlInitializer;
    private readonly IMongoDbInitializer _mongoDbInitializer;
    private readonly IPersistenceInitializationService _sut;

    public PersistenceInitializationServiceTests()
    {
        _postgreSqlInitializer = Substitute.For<IPostgreSqlInitializer>();
        _mongoDbInitializer = Substitute.For<IMongoDbInitializer>();
        _sut = new PersistenceInitializationService(_postgreSqlInitializer, _mongoDbInitializer);
    }

    [Fact]
    public async Task InitializeAsync_Should_Throw_ArgumentNullException_When_Services_Null()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger("Test");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.InitializeAsync(null!, config, logger));
    }

    [Fact]
    public async Task InitializeAsync_Should_Throw_ArgumentNullException_When_Configuration_Null()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger("Test");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.InitializeAsync(serviceProvider, null!, logger));
    }

    [Fact]
    public async Task InitializeAsync_Should_Throw_ArgumentNullException_When_Logger_Null()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var config = new ConfigurationBuilder().Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.InitializeAsync(serviceProvider, config, null!));
    }

    [Fact]
    public async Task InitializeAsync_Should_Always_Call_PostgreSql_Initializer()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var config = CreateConfiguration("PostgreSQL");
        var logger = Substitute.For<ILogger>();

        // Act
        await _sut.InitializeAsync(serviceProvider, config, logger);

        // Assert
        await _postgreSqlInitializer.Received(1)
            .InitializeIfEnabledAsync(serviceProvider, config, logger);
    }

    [Fact]
    public async Task InitializeAsync_Should_Not_Call_MongoDb_When_Provider_Is_Not_MongoDb()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var config = CreateConfiguration("PostgreSQL");
        var logger = Substitute.For<ILogger>();

        // Act
        await _sut.InitializeAsync(serviceProvider, config, logger);

        // Assert
        await _mongoDbInitializer.DidNotReceive()
            .InitializeAsync(Arg.Any<IServiceProvider>(), Arg.Any<IConfiguration>(), Arg.Any<ILogger>());
    }

    [Theory]
    [InlineData("MongoDb")]
    [InlineData("mongodb")]
    [InlineData("MONGODB")]
    public async Task InitializeAsync_Should_Call_MongoDb_When_Provider_Is_MongoDb(string provider)
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var config = CreateConfiguration(provider);
        var logger = Substitute.For<ILogger>();

        // Act
        await _sut.InitializeAsync(serviceProvider, config, logger);

        // Assert
        await _mongoDbInitializer.Received(1)
            .InitializeAsync(serviceProvider, config, logger);
    }

    [Fact]
    public async Task InitializeAsync_Should_Call_MongoDb_When_Persistence_Section_Is_Missing()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var config = new ConfigurationBuilder().Build();
        var logger = Substitute.For<ILogger>();

        // Act
        await _sut.InitializeAsync(serviceProvider, config, logger);

        // Assert
        await _mongoDbInitializer.Received(1)
            .InitializeAsync(serviceProvider, config, logger);
    }

    [Fact]
    public void Service_Should_Implement_Interface()
    {
        _sut.Should().BeAssignableTo<IPersistenceInitializationService>();
    }

    private static IConfiguration CreateConfiguration(string provider)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = provider
            })
            .Build();
    }
}

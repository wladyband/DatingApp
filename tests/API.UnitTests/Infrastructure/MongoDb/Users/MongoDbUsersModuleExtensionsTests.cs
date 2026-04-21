using API.Application.Ports.Persistence;
using API.Infrastructure.MongoDb.Persistence;
using API.Infrastructure.MongoDb.Users;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace API.UnitTests.Infrastructure.MongoDb.Users;

/// <summary>
/// Testes para validar o registro de serviços da extensão MongoDbUsersModule.
/// Estes testes garantem que os serviços corretos foram registrados no container de DI
/// com o lifetime esperado.
/// </summary>
public class MongoDbUsersModuleExtensionsTests
{
    [Fact]
    public void AddMongoDbUsersModule_ShouldRegisterMongoUserRepositoryAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMongoDbUsersModule();

        // Assert
        var descriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(MongoUserRepository));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMongoDbUsersModule_ShouldRegisterIUserRepositoryAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMongoDbUsersModule();

        // Assert
        var descriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(IUserRepository));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMongoDbUsersModule_ShouldRegisterIUserRepositoryUsingFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMongoDbUsersModule();

        // Assert
        var userRepositoryDescriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(IUserRepository));

        userRepositoryDescriptor.Should().NotBeNull();
        // Registrado como factory, então ImplementationFactory não é null
        userRepositoryDescriptor!.ImplementationFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddMongoDbUsersModule_ShouldReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddMongoDbUsersModule();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddMongoDbUsersModule_ShouldRegisterBothRepositoryAndInterface()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMongoDbUsersModule();

        // Assert
        var registeredServices = services.Where(sd =>
            sd.ServiceType == typeof(MongoUserRepository) ||
            sd.ServiceType == typeof(IUserRepository)).ToList();

        registeredServices.Should().HaveCount(2);
    }
}

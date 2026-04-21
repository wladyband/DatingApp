using API.Application;
using API.Application.UseCases.Account;
using API.Application.UseCases.Login;
using API.Application.UseCases.Users;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace API.UnitTests.Application;

/// <summary>
/// Testes para validar o registro de serviços da extensão ApplicationServiceExtensions.
/// Estes testes garantem que todos os use cases corretos foram registrados no container de DI
/// com o lifetime esperado.
/// </summary>
public class ApplicationServiceExtensionsTests
{
    [Fact]
    public void AddApplicationServices_ShouldRegisterCreateUserUseCase()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var descriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(CreateUserUseCase));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterGetUserByIdUseCase()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var descriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(GetUserByIdUseCase));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterGetAllUsersUseCase()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var descriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(GetAllUsersUseCase));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterDeleteUserUseCase()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var descriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(DeleteUserUseCase));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterCreateAccountUseCase()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var descriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(CreateAccountUseCase));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterLoginUseCase()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var descriptor = services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(LoginUseCase));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddApplicationServices_ShouldReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplicationServices();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterAllSixUseCases()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var expectedUseCases = new[]
        {
            typeof(CreateUserUseCase),
            typeof(GetUserByIdUseCase),
            typeof(GetAllUsersUseCase),
            typeof(DeleteUserUseCase),
            typeof(CreateAccountUseCase),
            typeof(LoginUseCase)
        };

        var registeredUseCases = services
            .Where(sd => expectedUseCases.Contains(sd.ServiceType))
            .ToList();

        registeredUseCases.Should().HaveCount(expectedUseCases.Length);
    }
}

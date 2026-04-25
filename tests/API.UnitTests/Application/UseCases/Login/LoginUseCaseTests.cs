using API.Application.DTOs.Requests.Login;
using API.Application.Ports.Infrastructure;
using API.Application.Ports.Services;
using API.Application.UseCases.Login;
using API.Domain.Entities;
using API.Domain.Exceptions;
using API.Domain.Services;
using FluentAssertions;
using NSubstitute;

namespace API.UnitTests.Application.UseCases.Login;

public class LoginUseCaseTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITokenService _tokenService;
    private readonly LoginUseCase _sut;

    public LoginUseCaseTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _tokenService = Substitute.For<ITokenService>();
        _tokenService.CreateToken(Arg.Any<AppUser>()).Returns("fake-token");
        _sut = new LoginUseCase(_accountRepository, _tokenService);
    }

    // ──────────────────────────────────────────────
    // Validações de entrada
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenInputIsNull_ThrowsArgumentNullException()
    {
        var act = () => _sut.LoginAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("input");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WhenEmailIsNullOrWhiteSpace_ThrowsDomainException(string? email)
    {
        var input = new LoginInput { Email = email!, Password = "qualquer" };

        var act = () => _sut.LoginAsync(input);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Email*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WhenPasswordIsNullOrWhiteSpace_ThrowsDomainException(string? password)
    {
        var input = new LoginInput { Email = "user@test.com", Password = password! };

        var act = () => _sut.LoginAsync(input);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*enha*");
    }

    // ──────────────────────────────────────────────
    // Usuário não encontrado
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenUserNotFound_ThrowsInvalidCredentialsException()
    {
        _accountRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((AppUser?)null);

        var input = new LoginInput { Email = "noexist@test.com", Password = "qualquer" };

        var act = () => _sut.LoginAsync(input);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task ExecuteAsync_NormalizesEmailBeforeQuerying()
    {
        _accountRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((AppUser?)null);

        var input = new LoginInput { Email = "  user@test.com  ", Password = "qualquer" };

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(input));

        await _accountRepository.Received(1).GetByEmailAsync("user@test.com", Arg.Any<CancellationToken>());
    }

    // ──────────────────────────────────────────────
    // Senha incorreta
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenPasswordIsWrong_ThrowsInvalidCredentialsException()
    {
        var user = CreateUserWithPassword("senhaCorreta");
        _accountRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(user);

        var input = new LoginInput { Email = "user@test.com", Password = "senhaErrada" };

        var act = () => _sut.LoginAsync(input);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    // ──────────────────────────────────────────────
    // Login bem-sucedido
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenCredentialsAreValid_ReturnsUser()
    {
        const string password = "senhaCorreta123";
        var user = CreateUserWithPassword(password);
        _accountRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(user);

        var input = new LoginInput { Email = "user@test.com", Password = password };

        var result = await _sut.LoginAsync(input);

        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.DisplayName.Should().Be(user.DisplayName);
        result.Token.Should().Be("fake-token");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCredentialsAreValid_QueriesRepositoryOnce()
    {
        const string password = "senhaCorreta123";
        var user = CreateUserWithPassword(password);
        _accountRepository.GetByEmailAsync("user@test.com", Arg.Any<CancellationToken>()).Returns(user);

        var input = new LoginInput { Email = "user@test.com", Password = password };

        await _sut.LoginAsync(input);

        await _accountRepository.Received(1).GetByEmailAsync("user@test.com", Arg.Any<CancellationToken>());
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static AppUser CreateUserWithPassword(string password)
    {
        var (hash, salt) = PasswordService.ComputePasswordHash(password);
        return new AppUser
        {
            DisplayName = "Test User",
            Email = "user@test.com",
            PasswordHash = hash,
            PasswordSalt = salt
        };
    }
}

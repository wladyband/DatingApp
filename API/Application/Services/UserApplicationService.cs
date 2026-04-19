using API.Application.Ports;
using API.Application.UseCases.Users;
using API.Core.Entities;

namespace API.Application.Services;

/// <summary>
/// Application Service para orquestração de operações de usuário.
/// Coordena múltiplos use cases e chamadas para ports externos.
/// </summary>
public class UserApplicationService
{
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly GetUserByIdUseCase _getUserByIdUseCase;
    private readonly GetAllUsersUseCase _getAllUsersUseCase;
    private readonly DeleteUserUseCase _deleteUserUseCase;
    private readonly IEmailService _emailService;

    public UserApplicationService(
        CreateUserUseCase createUserUseCase,
        GetUserByIdUseCase getUserByIdUseCase,
        GetAllUsersUseCase getAllUsersUseCase,
        DeleteUserUseCase deleteUserUseCase,
        IEmailService emailService)
    {
        _createUserUseCase = createUserUseCase;
        _getUserByIdUseCase = getUserByIdUseCase;
        _getAllUsersUseCase = getAllUsersUseCase;
        _deleteUserUseCase = deleteUserUseCase;
        _emailService = emailService;
    }

    public async Task<AppUser> CreateUserAsync(CreateUserInput input)
    {
        var user = await _createUserUseCase.ExecuteAsync(input);

        // Orquestração: pode enviar email, logs, etc.
        await _emailService.SendWelcomeEmailAsync(user.Email, user.Displayname);

        return user;
    }

    public async Task<AppUser?> GetUserByIdAsync(string userId) =>
        await _getUserByIdUseCase.ExecuteAsync(userId);

    public async Task<IEnumerable<AppUser>> GetAllUsersAsync() =>
        await _getAllUsersUseCase.ExecuteAsync();

    public async Task<bool> DeleteUserAsync(string userId) =>
        await _deleteUserUseCase.ExecuteAsync(userId);
}

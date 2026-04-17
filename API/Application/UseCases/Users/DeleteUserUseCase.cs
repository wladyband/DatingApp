using API.Application.Ports;

namespace API.Application.UseCases.Users;

public class DeleteUserUseCase
{
    private readonly IUserRepository _userRepository;

    public DeleteUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> ExecuteAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("ID do usuário é obrigatório.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        await _userRepository.RemoveAsync(userId);
        await _userRepository.SaveChangesAsync();

        return true;
    }
}

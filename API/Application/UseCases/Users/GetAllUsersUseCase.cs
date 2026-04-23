using API.Application.Ports.Services;
using API.Domain.Entities;

namespace API.Application.UseCases.Users;

public class GetAllUsersUseCase
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }
}



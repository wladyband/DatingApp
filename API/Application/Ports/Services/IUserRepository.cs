using API.Domain.Entities;

namespace API.Application.Ports.Services;

/// <summary>
/// Port que define como o domínio acessa dados de usuários.
/// Esta é uma abstração que isola o domínio da implementação específica de persistência.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtém um usuário por seu ID.
    /// </summary>
    Task<AppUser?> GetByIdAsync(string id);

    /// <summary>
    /// Obtém um usuário por seu email.
    /// </summary>
    Task<AppUser?> GetByEmailAsync(string email);



    Task AddAsync(AppUser user);

    Task<IEnumerable<AppUser>> GetAllAsync();

    Task UpdateAsync(AppUser user);

    Task RemoveAsync(string id);
}

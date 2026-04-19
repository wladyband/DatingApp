using API.Domain.Entities;

namespace API.Application.Ports.Persistence;

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

    /// <summary>
    /// Obtém todos os usuários.
    /// </summary>
    Task<IEnumerable<AppUser>> GetAllAsync();

    /// <summary>
    /// Adiciona um novo usuário ao repositório.
    /// </summary>
    Task AddAsync(AppUser user);

    /// <summary>
    /// Atualiza um usuário existente.
    /// </summary>
    Task UpdateAsync(AppUser user);

    /// <summary>
    /// Remove um usuário pelo ID.
    /// </summary>
    Task RemoveAsync(string id);
}




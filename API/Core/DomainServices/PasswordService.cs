using System.Security.Cryptography;

namespace API.Core.DomainServices;

/// <summary>
/// Domain Service para gerenciar hash e salt de senhas.
/// Concentra lógica criptográfica de domínio.
/// </summary>
public class PasswordService
{
    /// <summary>
    /// Computa hash e salt para uma senha em plain text.
    /// </summary>
    public static (byte[] hash, byte[] salt) ComputePasswordHash(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new ArgumentException("Senha não pode estar vazia.");

        using var hmac = new HMACSHA512();
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(plainPassword));
        var salt = hmac.Key;

        return (hash, salt);
    }

    /// <summary>
    /// Verifica se a senha fornecida corresponde ao hash armazenado.
    /// </summary>
    public static bool VerifyPassword(string plainPassword, byte[] hash, byte[] salt)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || hash == null || salt == null)
            return false;

        using var hmac = new HMACSHA512(salt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(plainPassword));

        return computedHash.SequenceEqual(hash);
    }
}

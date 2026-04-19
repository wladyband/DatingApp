namespace API.Domain.Exceptions;

/// <summary>
/// Exceção base para todos os erros de domínio.
/// Permite distinguir entre erros de negócio (domínio) e erros técnicos.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}

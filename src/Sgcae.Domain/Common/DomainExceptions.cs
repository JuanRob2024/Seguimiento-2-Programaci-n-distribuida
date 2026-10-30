namespace Sgcae.Domain.Common;

// EN: This is the base exception for every error raised by the domain.
// ES: Esta es la excepción base para todos los errores que lanza el dominio.
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}

// EN: I throw this exception when incoming data breaks a format rule (e.g. an invalid email).
// ES: Lanzo esta excepción cuando un dato de entrada incumple una regla de formato (por ejemplo, un correo inválido).
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message)
    {
    }
}

// EN: I throw this exception when an operation breaks a business invariant (e.g. blocking a revoked credential).
// ES: Lanzo esta excepción cuando una operación rompe una invariante de negocio (por ejemplo, bloquear una credencial revocada).
public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}

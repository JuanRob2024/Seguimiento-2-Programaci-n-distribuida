namespace Sgcae.Application.Common;

// EN: I throw this exception when a requested resource does not exist (the API turns it into HTTP 404).
// ES: Lanzo esta excepción cuando un recurso solicitado no existe (la API la convierte en HTTP 404).
public sealed class NotFoundException : Exception
{
    public NotFoundException(string resource, object key)
        : base($"{resource} with key '{key}' was not found.")
    {
    }
}

// EN: I throw this exception when the operation conflicts with existing data, e.g. a duplicated document (HTTP 409).
// ES: Lanzo esta excepción cuando la operación entra en conflicto con datos existentes, ej. un documento duplicado (HTTP 409).
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

// EN: I throw this exception when the request parameters are invalid, e.g. a wrong date range (HTTP 400).
// ES: Lanzo esta excepción cuando los parámetros de la solicitud son inválidos, ej. un rango de fechas erróneo (HTTP 400).
public sealed class RequestValidationException : Exception
{
    public RequestValidationException(string message) : base(message)
    {
    }
}

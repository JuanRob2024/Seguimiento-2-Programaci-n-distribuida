using Sgcae.Application.Abstractions;

namespace Sgcae.Infrastructure.Services;

// EN: This is the real implementation of the clock: it returns the current UTC time of the server.
// ES: Esta es la implementación real del reloj: retorna la hora UTC actual del servidor.
internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

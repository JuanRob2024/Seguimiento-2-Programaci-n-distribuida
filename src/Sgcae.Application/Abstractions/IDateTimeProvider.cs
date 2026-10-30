namespace Sgcae.Application.Abstractions;

// EN: This abstraction gives me the current date/time. Thanks to it the use cases do not depend on DateTime.UtcNow
//     directly, which makes them testable.
// ES: Esta abstracción me entrega la fecha/hora actual. Gracias a ella los casos de uso no dependen directamente de
//     DateTime.UtcNow, lo que los hace probables (testeables).
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

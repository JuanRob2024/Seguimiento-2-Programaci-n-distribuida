using Sgcae.Domain.Common;

namespace Sgcae.Domain.AccessPoints;

// EN: This value object represents the unique code of a physical access point (for example "RECEPTION-01").
// ES: Este objeto de valor representa el código único de un punto de acceso físico (por ejemplo "RECEPTION-01").
public sealed record AccessPointCode
{
    public const int MinLength = 3;
    public const int MaxLength = 30;

    public string Value { get; }

    private AccessPointCode(string value) => Value = value;

    public static AccessPointCode Create(string? value)
    {
        if (!TryCreate(value, out var code))
        {
            throw new DomainValidationException(
                $"The access point code must have between {MinLength} and {MaxLength} characters (letters, digits, '-' or '_').");
        }

        return code!;
    }

    // EN: Here I validate and normalize the code without throwing exceptions.
    // ES: Aquí valido y normalizo el código sin lanzar excepciones.
    public static bool TryCreate(string? value, out AccessPointCode? code)
    {
        code = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length is < MinLength or > MaxLength ||
            !normalized.All(c => char.IsLetterOrDigit(c) || c is '-' or '_'))
        {
            return false;
        }

        code = new AccessPointCode(normalized);
        return true;
    }

    public override string ToString() => Value;
}

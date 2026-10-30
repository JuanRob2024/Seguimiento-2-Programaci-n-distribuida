using Sgcae.Domain.Common;

namespace Sgcae.Domain.Credentials;

// EN: This value object represents the unique code of an access credential (for example "CRD-7F3A91B2").
// ES: Este objeto de valor representa el código único de una credencial de acceso (por ejemplo "CRD-7F3A91B2").
public sealed record CredentialCode
{
    public const int MinLength = 6;
    public const int MaxLength = 32;

    public string Value { get; }

    private CredentialCode(string value) => Value = value;

    // EN: Here I create the code validating length and allowed characters (letters, digits and '-').
    // ES: Aquí creo el código validando la longitud y los caracteres permitidos (letras, dígitos y '-').
    public static CredentialCode Create(string? value)
    {
        if (!TryCreate(value, out var code))
        {
            throw new DomainValidationException(
                $"The credential code must have between {MinLength} and {MaxLength} characters (letters, digits or '-').");
        }

        return code!;
    }

    // EN: This version does not throw; I use it when reading codes sent by access devices.
    // ES: Esta versión no lanza excepción; la uso al leer códigos enviados por los dispositivos de acceso.
    public static bool TryCreate(string? value, out CredentialCode? code)
    {
        code = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length is < MinLength or > MaxLength ||
            !normalized.All(c => char.IsLetterOrDigit(c) || c == '-'))
        {
            return false;
        }

        code = new CredentialCode(normalized);
        return true;
    }

    // EN: Here I generate a random code when the client does not send one.
    // ES: Aquí genero un código aleatorio cuando el cliente no envía uno.
    public static CredentialCode Generate() =>
        new($"CRD-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}");

    public override string ToString() => Value;
}

using Sgcae.Domain.Common;

namespace Sgcae.Domain.Employees;

// EN: This value object represents the employee's unique identification number.
//     It is immutable and it can only be created through Create(), which validates its format.
// ES: Este objeto de valor representa el número de identificación único del empleado.
//     Es inmutable y solo se puede crear mediante Create(), que valida su formato.
public sealed record DocumentNumber
{
    public const int MinLength = 5;
    public const int MaxLength = 20;

    public string Value { get; }

    private DocumentNumber(string value) => Value = value;

    // EN: Here I create the document number; I throw an exception if the format is invalid.
    // ES: Aquí creo el número de documento; lanzo una excepción si el formato es inválido.
    public static DocumentNumber Create(string? value)
    {
        if (!TryCreate(value, out var documentNumber))
        {
            throw new DomainValidationException(
                $"The document number must have between {MinLength} and {MaxLength} letters or digits.");
        }

        return documentNumber!;
    }

    // EN: Here I normalize the document (trim + upper case) and verify it only has letters and digits, without throwing.
    // ES: Aquí normalizo el documento (sin espacios + mayúsculas) y verifico que solo tenga letras y dígitos, sin lanzar excepción.
    public static bool TryCreate(string? value, out DocumentNumber? documentNumber)
    {
        documentNumber = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length is < MinLength or > MaxLength || !normalized.All(char.IsLetterOrDigit))
        {
            return false;
        }

        documentNumber = new DocumentNumber(normalized);
        return true;
    }

    public override string ToString() => Value;
}

using System.Text.RegularExpressions;
using Sgcae.Domain.Common;

namespace Sgcae.Domain.Employees;

// EN: This value object represents a corporate email. It guarantees a valid format and stores it in lower case.
// ES: Este objeto de valor representa un correo electrónico. Garantiza un formato válido y lo guarda en minúsculas.
public sealed partial record Email
{
    public const int MaxLength = 150;

    public string Value { get; }

    private Email(string value) => Value = value;

    // EN: Here I validate the email with a simple regular expression and normalize it.
    // ES: Aquí valido el correo con una expresión regular sencilla y lo normalizo.
    public static Email Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException("The email is required.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > MaxLength || !EmailRegex().IsMatch(normalized))
        {
            throw new DomainValidationException($"The email '{value}' is not valid.");
        }

        return new Email(normalized);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public override string ToString() => Value;
}

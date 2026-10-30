namespace Sgcae.Domain.Common;

// EN: This helper centralizes the basic validations used by entities and value objects.
// ES: Este helper centraliza las validaciones básicas que usan las entidades y los objetos de valor.
public static class Guard
{
    // EN: Here I check that a text is not empty and does not exceed the maximum length; I return it trimmed.
    // ES: Aquí verifico que un texto no esté vacío y no supere la longitud máxima; lo retorno sin espacios sobrantes.
    public static string RequiredText(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"The field '{fieldName}' is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainValidationException(
                $"The field '{fieldName}' cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }

    // EN: Here I validate an optional text: null or empty becomes null, otherwise I check its length.
    // ES: Aquí valido un texto opcional: si es nulo o vacío retorno null, si no, verifico su longitud.
    public static string? OptionalText(string? value, string fieldName, int maxLength)
    {
        return string.IsNullOrWhiteSpace(value) ? null : RequiredText(value, fieldName, maxLength);
    }
}

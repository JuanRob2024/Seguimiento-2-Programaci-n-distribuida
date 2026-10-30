using Sgcae.Domain.Common;

namespace Sgcae.Domain.AccessPoints;

// EN: This is the AccessPoint aggregate root (reception, parking, warehouse, restricted areas...).
//     It knows whether it is enabled; a disabled access point rejects every movement.
// ES: Esta es la raíz de agregado Punto de Acceso (recepción, parqueadero, bodega, áreas restringidas...).
//     Sabe si está habilitado; un punto de acceso deshabilitado rechaza todos los movimientos.
public sealed class AccessPoint : AggregateRoot
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 250;

    public AccessPointCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public AccessPointType Type { get; private set; }
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // EN: Constructor reserved for Entity Framework.
    // ES: Constructor reservado para Entity Framework.
    private AccessPoint()
    {
    }

    // EN: This factory method registers a new access point. It starts enabled.
    // ES: Este método de fábrica registra un nuevo punto de acceso. Inicia habilitado.
    public static AccessPoint Register(
        AccessPointCode code,
        string name,
        AccessPointType type,
        string? description,
        DateTime nowUtc)
    {
        if (!Enum.IsDefined(type))
        {
            throw new DomainValidationException($"The access point type '{type}' is not valid.");
        }

        return new AccessPoint
        {
            Code = code ?? throw new DomainValidationException("The access point code is required."),
            Name = Guard.RequiredText(name, nameof(Name), NameMaxLength),
            Type = type,
            Description = Guard.OptionalText(description, nameof(Description), DescriptionMaxLength),
            IsEnabled = true,
            CreatedAtUtc = nowUtc
        };
    }

    // EN: Here I enable the access point.
    // ES: Aquí habilito el punto de acceso.
    public void Enable()
    {
        if (IsEnabled)
        {
            throw new BusinessRuleException("The access point is already enabled.");
        }

        IsEnabled = true;
    }

    // EN: Here I disable the access point (for example, during maintenance).
    // ES: Aquí deshabilito el punto de acceso (por ejemplo, durante un mantenimiento).
    public void Disable()
    {
        if (!IsEnabled)
        {
            throw new BusinessRuleException("The access point is already disabled.");
        }

        IsEnabled = false;
    }
}

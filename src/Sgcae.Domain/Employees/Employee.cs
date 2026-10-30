using Sgcae.Domain.Common;

namespace Sgcae.Domain.Employees;

// EN: This is the Employee aggregate root. It protects its own invariants:
//     every field is validated and the state can only change through its methods (private setters).
// ES: Esta es la raíz de agregado Empleado. Protege sus propias invariantes:
//     cada campo se valida y el estado solo cambia a través de sus métodos (setters privados).
public sealed class Employee : AggregateRoot
{
    public const int NamesMaxLength = 100;
    public const int PositionMaxLength = 100;
    public const int AreaMaxLength = 100;

    public DocumentNumber DocumentNumber { get; private set; } = null!;
    public string FirstNames { get; private set; } = string.Empty;
    public string LastNames { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string Position { get; private set; } = string.Empty;
    public string Area { get; private set; } = string.Empty;
    public EmployeeStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }

    public bool IsActive => Status == EmployeeStatus.Active;

    // EN: This empty constructor is only used by Entity Framework to rebuild the object from the database.
    // ES: Este constructor vacío solo lo usa Entity Framework para reconstruir el objeto desde la base de datos.
    private Employee()
    {
    }

    // EN: This factory method registers a new employee. Every new employee starts in the Active status.
    // ES: Este método de fábrica registra un nuevo empleado. Todo empleado nuevo inicia en estado Activo.
    public static Employee Register(
        DocumentNumber documentNumber,
        string firstNames,
        string lastNames,
        Email email,
        string position,
        string area,
        DateTime nowUtc)
    {
        var employee = new Employee
        {
            DocumentNumber = documentNumber ?? throw new DomainValidationException("The document number is required."),
            Status = EmployeeStatus.Active,
            CreatedAtUtc = nowUtc
        };

        employee.SetInformation(firstNames, lastNames, email, position, area);
        return employee;
    }

    // EN: Here I update the employee's information. The document number is immutable on purpose.
    // ES: Aquí actualizo la información del empleado. El número de documento es inmutable a propósito.
    public void UpdateInformation(
        string firstNames,
        string lastNames,
        Email email,
        string position,
        string area,
        DateTime nowUtc)
    {
        SetInformation(firstNames, lastNames, email, position, area);
        UpdatedAtUtc = nowUtc;
    }

    // EN: Here I deactivate the employee. An inactive employee can never be granted access.
    // ES: Aquí desactivo al empleado. A un empleado inactivo nunca se le puede autorizar un acceso.
    public void Deactivate(DateTime nowUtc)
    {
        if (!IsActive)
        {
            throw new BusinessRuleException("The employee is already inactive.");
        }

        Status = EmployeeStatus.Inactive;
        DeactivatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    // EN: This private method validates and assigns the editable fields (shared by Register and Update).
    // ES: Este método privado valida y asigna los campos editables (lo comparten Register y Update).
    private void SetInformation(string firstNames, string lastNames, Email email, string position, string area)
    {
        FirstNames = Guard.RequiredText(firstNames, nameof(FirstNames), NamesMaxLength);
        LastNames = Guard.RequiredText(lastNames, nameof(LastNames), NamesMaxLength);
        Email = email ?? throw new DomainValidationException("The email is required.");
        Position = Guard.RequiredText(position, nameof(Position), PositionMaxLength);
        Area = Guard.RequiredText(area, nameof(Area), AreaMaxLength);
    }
}

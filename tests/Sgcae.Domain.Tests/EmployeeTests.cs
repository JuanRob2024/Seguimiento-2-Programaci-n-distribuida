using Sgcae.Domain.Common;
using Sgcae.Domain.Employees;

namespace Sgcae.Domain.Tests;

// EN: These tests verify the invariants of the Employee aggregate.
// ES: Estas pruebas verifican las invariantes del agregado Empleado.
public class EmployeeTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    internal static Employee NewEmployee() => Employee.Register(
        DocumentNumber.Create("1037654321"), "Juan Pablo", "Robledo Urrego",
        Email.Create("juan@example.com"), "Analyst", "Logistics", Now);

    [Fact]
    public void Register_CreatesActiveEmployee()
    {
        var employee = NewEmployee();

        Assert.True(employee.IsActive);
        Assert.Equal(EmployeeStatus.Active, employee.Status);
        Assert.NotEqual(Guid.Empty, employee.Id);
    }

    [Fact]
    public void Register_WithEmptyName_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Employee.Register(
            DocumentNumber.Create("1037654321"), " ", "Robledo",
            Email.Create("juan@example.com"), "Analyst", "Logistics", Now));
    }

    [Fact]
    public void Deactivate_ChangesStatusToInactive()
    {
        var employee = NewEmployee();

        employee.Deactivate(Now.AddDays(1));

        Assert.False(employee.IsActive);
        Assert.Equal(Now.AddDays(1), employee.DeactivatedAtUtc);
    }

    [Fact]
    public void Deactivate_Twice_Throws()
    {
        var employee = NewEmployee();
        employee.Deactivate(Now);

        Assert.Throws<BusinessRuleException>(() => employee.Deactivate(Now));
    }

    [Fact]
    public void UpdateInformation_ChangesDataButNotDocument()
    {
        var employee = NewEmployee();

        employee.UpdateInformation("Juan", "Robledo", Email.Create("new@example.com"), "Coordinator", "Security", Now);

        Assert.Equal("Coordinator", employee.Position);
        Assert.Equal("new@example.com", employee.Email.Value);
        Assert.Equal("1037654321", employee.DocumentNumber.Value);
        Assert.NotNull(employee.UpdatedAtUtc);
    }
}

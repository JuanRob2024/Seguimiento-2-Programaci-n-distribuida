using Sgcae.Domain.AccessLogs;
using Sgcae.Domain.AccessPoints;

namespace Sgcae.Domain.Tests;

// EN: These tests verify each rule of the AccessPolicy domain service.
// ES: Estas pruebas verifican cada regla del servicio de dominio AccessPolicy.
public class AccessPolicyTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    private static AccessPoint NewAccessPoint() =>
        AccessPoint.Register(AccessPointCode.Create("RECEPTION-01"), "Reception", AccessPointType.Reception, null, Now);

    [Fact]
    public void ValidEntry_IsGranted()
    {
        var decision = AccessPolicy.Evaluate(
            NewAccessPoint(), CredentialTests.NewCredential(), EmployeeTests.NewEmployee(), MovementType.Entry, null, Now);

        Assert.True(decision.IsGranted);
        Assert.Null(decision.DenialReason);
    }

    [Fact]
    public void DisabledAccessPoint_IsDenied()
    {
        var accessPoint = NewAccessPoint();
        accessPoint.Disable();

        var decision = AccessPolicy.Evaluate(
            accessPoint, CredentialTests.NewCredential(), EmployeeTests.NewEmployee(), MovementType.Entry, null, Now);

        Assert.Equal(AccessDenialReason.AccessPointDisabled, decision.DenialReason);
    }

    [Fact]
    public void UnknownCredential_IsDenied()
    {
        var decision = AccessPolicy.Evaluate(NewAccessPoint(), null, null, MovementType.Entry, null, Now);

        Assert.Equal(AccessDenialReason.CredentialNotFound, decision.DenialReason);
    }

    [Fact]
    public void BlockedCredential_IsDenied()
    {
        var credential = CredentialTests.NewCredential();
        credential.Block(null, Now);

        var decision = AccessPolicy.Evaluate(
            NewAccessPoint(), credential, EmployeeTests.NewEmployee(), MovementType.Entry, null, Now);

        Assert.Equal(AccessDenialReason.CredentialBlocked, decision.DenialReason);
    }

    [Fact]
    public void ExpiredCredential_IsDenied()
    {
        var credential = CredentialTests.NewCredential(Now.AddHours(1));

        var decision = AccessPolicy.Evaluate(
            NewAccessPoint(), credential, EmployeeTests.NewEmployee(), MovementType.Entry, null, Now.AddHours(2));

        Assert.Equal(AccessDenialReason.CredentialExpired, decision.DenialReason);
    }

    [Fact]
    public void InactiveEmployee_IsDenied()
    {
        var employee = EmployeeTests.NewEmployee();
        employee.Deactivate(Now);

        var decision = AccessPolicy.Evaluate(
            NewAccessPoint(), CredentialTests.NewCredential(), employee, MovementType.Entry, null, Now);

        Assert.Equal(AccessDenialReason.EmployeeInactive, decision.DenialReason);
    }

    [Fact]
    public void DoubleEntry_IsDenied()
    {
        var decision = AccessPolicy.Evaluate(
            NewAccessPoint(), CredentialTests.NewCredential(), EmployeeTests.NewEmployee(),
            MovementType.Entry, MovementType.Entry, Now);

        Assert.Equal(AccessDenialReason.AlreadyInside, decision.DenialReason);
    }

    [Fact]
    public void ExitWithoutEntry_IsDenied()
    {
        var decision = AccessPolicy.Evaluate(
            NewAccessPoint(), CredentialTests.NewCredential(), EmployeeTests.NewEmployee(),
            MovementType.Exit, null, Now);

        Assert.Equal(AccessDenialReason.NotInside, decision.DenialReason);
    }

    [Fact]
    public void ExitAfterEntry_IsGranted()
    {
        var decision = AccessPolicy.Evaluate(
            NewAccessPoint(), CredentialTests.NewCredential(), EmployeeTests.NewEmployee(),
            MovementType.Exit, MovementType.Entry, Now);

        Assert.True(decision.IsGranted);
    }

    [Fact]
    public void DeniedDecision_IsRecordedInAccessLog()
    {
        // EN: Denied attempts must also be stored, with their reason (traceability).
        // ES: Los intentos rechazados también deben guardarse, con su motivo (trazabilidad).
        var log = AccessLog.Record("crd-unknown", "reception-01", null, null, null, MovementType.Entry,
            AccessDecision.Denied(AccessDenialReason.CredentialNotFound), Now);

        Assert.Equal(AccessResult.Denied, log.Result);
        Assert.Equal(AccessDenialReason.CredentialNotFound, log.DenialReason);
        Assert.Equal("CRD-UNKNOWN", log.RequestedCredentialCode);
    }
}

using Sgcae.Domain.Common;
using Sgcae.Domain.Credentials;

namespace Sgcae.Domain.Tests;

// EN: These tests verify the status transitions of a credential.
// ES: Estas pruebas verifican las transiciones de estado de una credencial.
public class CredentialTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    internal static Credential NewCredential(DateTime? expiresAtUtc = null) =>
        Credential.Issue(Guid.NewGuid(), CredentialCode.Create("CRD-TEST0001"), Now, expiresAtUtc);

    [Fact]
    public void Issue_CreatesUsableCredential()
    {
        var credential = NewCredential();

        Assert.Equal(CredentialStatus.Active, credential.Status);
        Assert.True(credential.CanBeUsed(Now));
    }

    [Fact]
    public void Issue_WithPastExpiration_Throws()
    {
        Assert.Throws<DomainValidationException>(() => NewCredential(Now.AddMinutes(-1)));
    }

    [Fact]
    public void ExpiredCredential_CanNotBeUsed()
    {
        var credential = NewCredential(Now.AddDays(1));

        Assert.False(credential.CanBeUsed(Now.AddDays(2)));
        Assert.True(credential.IsExpired(Now.AddDays(2)));
    }

    [Fact]
    public void Block_ThenUnblock_ReturnsToActive()
    {
        var credential = NewCredential();

        credential.Block("Lost card", Now);
        Assert.Equal(CredentialStatus.Blocked, credential.Status);
        Assert.False(credential.CanBeUsed(Now));
        Assert.Equal("Lost card", credential.StatusReason);

        credential.Unblock(Now);
        Assert.Equal(CredentialStatus.Active, credential.Status);
    }

    [Fact]
    public void Unblock_ActiveCredential_Throws()
    {
        Assert.Throws<BusinessRuleException>(() => NewCredential().Unblock(Now));
    }

    [Fact]
    public void RevokedCredential_IsFinalState()
    {
        var credential = NewCredential();
        credential.Revoke("Employee left", Now);

        Assert.Throws<BusinessRuleException>(() => credential.Block(null, Now));
        Assert.Throws<BusinessRuleException>(() => credential.Unblock(Now));
        Assert.Throws<BusinessRuleException>(() => credential.Revoke(null, Now));
    }
}

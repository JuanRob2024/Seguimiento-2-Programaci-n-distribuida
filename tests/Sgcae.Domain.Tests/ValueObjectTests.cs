using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Common;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Employees;

namespace Sgcae.Domain.Tests;

// EN: These tests verify that the value objects validate and normalize their data.
// ES: Estas pruebas verifican que los objetos de valor validen y normalicen sus datos.
public class ValueObjectTests
{
    [Fact]
    public void Email_IsNormalizedToLowerCase()
    {
        var email = Email.Create("  Juan.Robledo@Example.COM ");
        Assert.Equal("juan.robledo@example.com", email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("a@b")]
    public void Email_WithInvalidFormat_Throws(string value)
    {
        Assert.Throws<DomainValidationException>(() => Email.Create(value));
    }

    [Fact]
    public void DocumentNumber_RejectsSymbols()
    {
        Assert.Throws<DomainValidationException>(() => DocumentNumber.Create("103.765.432"));
    }

    [Fact]
    public void ValueObjects_WithSameValue_AreEqual()
    {
        // EN: Value objects are compared by value, not by reference.
        // ES: Los objetos de valor se comparan por valor, no por referencia.
        Assert.Equal(DocumentNumber.Create("abc12345"), DocumentNumber.Create("ABC12345"));
    }

    [Fact]
    public void CredentialCode_Generate_ProducesValidCode()
    {
        var code = CredentialCode.Generate();
        Assert.True(CredentialCode.TryCreate(code.Value, out _));
        Assert.StartsWith("CRD-", code.Value);
    }

    [Fact]
    public void AccessPointCode_TryCreate_ReturnsFalseForInvalidCode()
    {
        Assert.False(AccessPointCode.TryCreate("A!", out var code));
        Assert.Null(code);
    }
}

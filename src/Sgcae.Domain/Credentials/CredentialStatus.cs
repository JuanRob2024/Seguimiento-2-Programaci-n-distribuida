namespace Sgcae.Domain.Credentials;

// EN: This enum represents the lifecycle of a credential: Active -> Blocked <-> Active -> Revoked (final state).
// ES: Este enum representa el ciclo de vida de una credencial: Activa -> Bloqueada <-> Activa -> Revocada (estado final).
public enum CredentialStatus
{
    Active = 1,
    Blocked = 2,
    Revoked = 3
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sgcae.Domain.AccessLogs;
using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Employees;

namespace Sgcae.Infrastructure.Persistence.Configurations;

// EN: This class maps the AccessLog aggregate to the "AccessLogs" table, with the indexes needed by the
//     history queries (by employee, by access point and by date).
// ES: Esta clase mapea el agregado Registro de Acceso a la tabla "AccessLogs", con los índices que necesitan
//     las consultas del historial (por empleado, por punto de acceso y por fecha).
internal sealed class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        builder.ToTable("AccessLogs");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.RequestedCredentialCode).HasMaxLength(AccessLog.RequestedCodeMaxLength).IsRequired();
        builder.Property(l => l.RequestedAccessPointCode).HasMaxLength(AccessLog.RequestedCodeMaxLength).IsRequired();
        builder.Property(l => l.Movement).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(l => l.Result).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(l => l.DenialReason).HasConversion<string>().HasMaxLength(30);

        // EN: Optional foreign keys: a denied attempt may have an unknown credential or access point.
        //     I use Restrict so the audit history can never be deleted in cascade.
        // ES: Llaves foráneas opcionales: un intento rechazado puede tener una credencial o un punto desconocido.
        //     Uso Restrict para que el historial de auditoría nunca se borre en cascada.
        builder.HasOne<Employee>().WithMany().HasForeignKey(l => l.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Credential>().WithMany().HasForeignKey(l => l.CredentialId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AccessPoint>().WithMany().HasForeignKey(l => l.AccessPointId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.OccurredAtUtc);
        builder.HasIndex(l => new { l.EmployeeId, l.OccurredAtUtc });
        builder.HasIndex(l => new { l.AccessPointId, l.OccurredAtUtc });
        builder.HasIndex(l => new { l.EmployeeId, l.AccessPointId, l.Result, l.OccurredAtUtc });
    }
}

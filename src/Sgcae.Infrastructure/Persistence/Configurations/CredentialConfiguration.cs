using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Employees;

namespace Sgcae.Infrastructure.Persistence.Configurations;

// EN: This class maps the Credential aggregate to the "Credentials" table.
// ES: Esta clase mapea el agregado Credencial a la tabla "Credentials".
internal sealed class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.ToTable("Credentials");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Code)
            .HasConversion(v => v.Value, v => CredentialCode.Create(v))
            .HasMaxLength(CredentialCode.MaxLength)
            .IsRequired();

        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.StatusReason).HasMaxLength(Credential.ReasonMaxLength);

        // EN: Aggregates reference each other only by Id. Here I create the foreign key without a navigation property.
        // ES: Los agregados se referencian entre sí solo por Id. Aquí creo la llave foránea sin propiedad de navegación.
        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => new { c.EmployeeId, c.Status });
    }
}

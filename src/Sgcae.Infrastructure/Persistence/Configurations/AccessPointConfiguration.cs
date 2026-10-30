using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sgcae.Domain.AccessPoints;

namespace Sgcae.Infrastructure.Persistence.Configurations;

// EN: This class maps the AccessPoint aggregate to the "AccessPoints" table.
// ES: Esta clase mapea el agregado Punto de Acceso a la tabla "AccessPoints".
internal sealed class AccessPointConfiguration : IEntityTypeConfiguration<AccessPoint>
{
    public void Configure(EntityTypeBuilder<AccessPoint> builder)
    {
        builder.ToTable("AccessPoints");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Code)
            .HasConversion(v => v.Value, v => AccessPointCode.Create(v))
            .HasMaxLength(AccessPointCode.MaxLength)
            .IsRequired();

        builder.Property(a => a.Name).HasMaxLength(AccessPoint.NameMaxLength).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(AccessPoint.DescriptionMaxLength);
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.HasIndex(a => a.Code).IsUnique();
        builder.HasIndex(a => a.IsEnabled);
    }
}

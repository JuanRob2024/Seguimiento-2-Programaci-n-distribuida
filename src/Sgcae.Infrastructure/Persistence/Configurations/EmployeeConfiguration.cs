using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sgcae.Domain.Employees;

namespace Sgcae.Infrastructure.Persistence.Configurations;

// EN: This class maps the Employee aggregate to the "Employees" table. I keep the mapping out of the domain
//     (Fluent API) so the domain stays free of persistence attributes.
// ES: Esta clase mapea el agregado Empleado a la tabla "Employees". Dejo el mapeo fuera del dominio
//     (Fluent API) para que el dominio quede libre de atributos de persistencia.
internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // EN: Value objects are stored as simple columns thanks to value converters.
        // ES: Los objetos de valor se guardan como columnas simples gracias a los convertidores de valor.
        builder.Property(e => e.DocumentNumber)
            .HasConversion(v => v.Value, v => DocumentNumber.Create(v))
            .HasMaxLength(DocumentNumber.MaxLength)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasConversion(v => v.Value, v => Email.Create(v))
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder.Property(e => e.FirstNames).HasMaxLength(Employee.NamesMaxLength).IsRequired();
        builder.Property(e => e.LastNames).HasMaxLength(Employee.NamesMaxLength).IsRequired();
        builder.Property(e => e.Position).HasMaxLength(Employee.PositionMaxLength).IsRequired();
        builder.Property(e => e.Area).HasMaxLength(Employee.AreaMaxLength).IsRequired();

        // EN: I store enums as text so the table is readable directly in SQL Server.
        // ES: Guardo los enums como texto para que la tabla sea legible directamente en SQL Server.
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Ignore(e => e.IsActive);

        // EN: Unique indexes guarantee uniqueness also at the database level.
        // ES: Los índices únicos garantizan la unicidad también a nivel de base de datos.
        builder.HasIndex(e => e.DocumentNumber).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => new { e.Status, e.Area });
    }
}

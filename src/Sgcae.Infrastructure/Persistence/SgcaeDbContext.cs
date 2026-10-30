using Microsoft.EntityFrameworkCore;
using Sgcae.Domain.AccessLogs;
using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Employees;
using Sgcae.Domain.Repositories;

namespace Sgcae.Infrastructure.Persistence;

// EN: This is the Entity Framework Core database context. It also acts as the Unit of Work:
//     SaveChangesAsync persists every pending change of a use case in a single transaction.
// ES: Este es el contexto de base de datos de Entity Framework Core. También actúa como Unidad de Trabajo:
//     SaveChangesAsync persiste todos los cambios pendientes de un caso de uso en una sola transacción.
public sealed class SgcaeDbContext : DbContext, IUnitOfWork
{
    public SgcaeDbContext(DbContextOptions<SgcaeDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<AccessPoint> AccessPoints => Set<AccessPoint>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    // EN: Here I apply every IEntityTypeConfiguration class found in this assembly (folder Configurations).
    // ES: Aquí aplico todas las clases IEntityTypeConfiguration que hay en este ensamblado (carpeta Configurations).
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SgcaeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgcae.Domain.AccessPoints;

namespace Sgcae.Infrastructure.Persistence;

// EN: This class prepares the database when the API starts: it applies the EF Core migrations and
//     creates some initial access points so the API can be tested right away.
// ES: Esta clase prepara la base de datos cuando la API inicia: aplica las migraciones de EF Core y
//     crea algunos puntos de acceso iniciales para poder probar la API de inmediato.
public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SgcaeDbContext>();

        // EN: If the project has migrations I apply them (recommended). If not, I create the schema directly.
        // ES: Si el proyecto tiene migraciones las aplico (recomendado). Si no, creo el esquema directamente.
        if (db.Database.GetMigrations().Any())
        {
            await db.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }

        await SeedAccessPointsAsync(db, cancellationToken);
    }

    // EN: Here I insert the typical access points of the case study only if the table is empty.
    // ES: Aquí inserto los puntos de acceso típicos del caso de estudio solo si la tabla está vacía.
    private static async Task SeedAccessPointsAsync(SgcaeDbContext db, CancellationToken cancellationToken)
    {
        if (await db.AccessPoints.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        db.AccessPoints.AddRange(
            AccessPoint.Register(AccessPointCode.Create("RECEPTION-01"), "Main reception", AccessPointType.Reception, "Main entrance of the headquarters", now),
            AccessPoint.Register(AccessPointCode.Create("PARKING-01"), "Parking lot", AccessPointType.Parking, "Vehicle and motorcycle parking", now),
            AccessPoint.Register(AccessPointCode.Create("WAREHOUSE-01"), "Warehouse", AccessPointType.Warehouse, "Storage warehouse", now),
            AccessPoint.Register(AccessPointCode.Create("RESTRICTED-01"), "Server room", AccessPointType.RestrictedArea, "Restricted area - IT only", now));

        await db.SaveChangesAsync(cancellationToken);
    }
}

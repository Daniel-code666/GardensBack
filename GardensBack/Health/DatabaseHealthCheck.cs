using GardensBack.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GardensBack.Health
{
    /// <summary>
    /// Comprueba la disponibilidad de la base de datos y la ausencia de migraciones pendientes.
    /// </summary>
    /// <param name="db">Contexto de base de datos utilizado por la comprobación.</param>
    public class DatabaseHealthCheck(ApplicationDbContext db) : IHealthCheck
    {
        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if ((await db.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                    return HealthCheckResult.Unhealthy("Pending migrations");

                _ = await db.Users.AnyAsync(cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("DB Unavailable", ex);
            }
        }
    }
}

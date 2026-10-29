using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GardensBack.Infrastructure.Persistance.Repositories
{
    public class UnitOfWork(ApplicationDbContext db, ILogger<UnitOfWork> logger) : IUnitOfWork
    {
        private readonly ApplicationDbContext _db = db;
        private readonly ILogger<UnitOfWork> _logger = logger;

        public async Task<OperationResult<T>> ExecuteCommandAsync<T>(string operationName, Func<CancellationToken, Task<T>> operation, CancellationToken ct)
        {
            try
            {
                var result = await operation(ct);

                await _db.SaveChangesAsync(ct);

                return OperationResult<T>.Success(operationName, result);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Conflicto de concurrencia durante {OperationName}.",
                    operationName);

                _db.ChangeTracker.Clear();

                return OperationResult<T>.Failure(
                    operationName,
                    40901,
                    "La información fue modificada por otra operación.");
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException postgresException)
            {
                _logger.LogWarning(
                    ex,
                    "PostgreSQL rechazó la operación {OperationName}. SQLSTATE: {SqlState}. Restricción: {Constraint}.",
                    operationName,
                    postgresException.SqlState,
                    postgresException.ConstraintName);

                _db.ChangeTracker.Clear();

                return MapPostgresException<T>(
                    operationName,
                    postgresException);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "EF Core no pudo guardar la operación {OperationName}.",
                    operationName);

                _db.ChangeTracker.Clear();

                return OperationResult<T>.Failure(
                    operationName,
                    50001,
                    "No fue posible guardar la información.");
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(
                    ex,
                    "Fallo de conexión con PostgreSQL durante {OperationName}.",
                    operationName);

                _db.ChangeTracker.Clear();

                return OperationResult<T>.Failure(
                    operationName,
                    50301,
                    "La base de datos no se encuentra disponible.");
            }
        }

        private static OperationResult<T> MapPostgresException<T>(
        string operationName,
        PostgresException exception)
        {
            return exception.SqlState switch
            {
                PostgresErrorCodes.UniqueViolation =>
                    OperationResult<T>.Failure(
                        operationName,
                        23505,
                        "Ya existe un registro con los mismos datos únicos."),

                PostgresErrorCodes.ForeignKeyViolation =>
                    OperationResult<T>.Failure(
                        operationName,
                        23503,
                        "El registro relacionado no existe."),

                PostgresErrorCodes.NotNullViolation =>
                    OperationResult<T>.Failure(
                        operationName,
                        23502,
                        "No se proporcionó un dato obligatorio."),

                PostgresErrorCodes.CheckViolation =>
                    OperationResult<T>.Failure(
                        operationName,
                        23514,
                        "Los datos no cumplen una restricción de la base de datos."),

                _ =>
                    OperationResult<T>.Failure(
                        operationName,
                        50002,
                        "La base de datos rechazó la operación.")
            };
        }
    }
}

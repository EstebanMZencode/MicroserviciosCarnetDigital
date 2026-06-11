using Dapper;
using MicroServicioRoles.Entities;
using System.Data;

namespace MicroServicioRoles.Repository
{
    // Repositorio de roles. Ejecuta todos los queries SQL contra el schema
    // [Carnet_Access_User] de la base Carnet_Access usando Dapper.
    // No utiliza procedimientos almacenados.
    // IMPORTANTE: Las tablas Roles y PantallasXRoles tienen triggers INSTEAD OF DELETE
    // que bloquean el borrado físico. Las eliminaciones se realizan como soft delete (Estado = 0).
    public class RolesRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public RolesRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        // Verifica que el rol exista y esté activo en base de datos.
        public async Task<bool> ExisteRolAsync(Guid rolId)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM [Carnet_Access_User].[Roles]
                WHERE RolID = @RolID AND Estado = 1";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            return await connection.ExecuteScalarAsync<int>(sql, new { RolID = rolId }) > 0;
        }

        // Verifica que TODAS las pantallas de la lista existan y estén activas.
        // Retorna true solo si todas existen; false si alguna falta.
        public async Task<bool> ExistenPantallasAsync(List<Guid> pantallaIds)
        {
            const string sql = @"
                SELECT COUNT(DISTINCT PantallaID)
                FROM [Carnet_Access_User].[Pantallas]
                WHERE PantallaID IN @PantallaIds AND Estado = 1";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            var encontradas = await connection.ExecuteScalarAsync<int>(sql, new { PantallaIds = pantallaIds });
            return encontradas == pantallaIds.Count;
        }

        // Recupera un rol activo con sus pantallas activas asignadas.
        // Retorna null si no existe o está inactivo.
        public async Task<RolResponse?> GetRolPorIdAsync(Guid rolId)
        {
            const string sqlRol = @"
                SELECT RolID, NombreRol
                FROM [Carnet_Access_User].[Roles]
                WHERE RolID = @RolID AND Estado = 1";

            const string sqlPantallas = @"
                SELECT PantallaID
                FROM [Carnet_Access_User].[PantallasXRoles]
                WHERE RolID = @RolID AND Estado = 1";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();

            var rol = await connection.QueryFirstOrDefaultAsync<Rol>(sqlRol, new { RolID = rolId });
            if (rol is null) return null;

            var pantallas = await connection.QueryAsync<Guid>(sqlPantallas, new { RolID = rolId });

            return new RolResponse
            {
                RolID = rol.RolID,
                NombreRol = rol.NombreRol,
                Pantallas = pantallas.ToList()
            };
        }

        // Recupera el total de roles activos para calcular la paginación.
        public async Task<int> GetTotalRolesAsync()
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM [Carnet_Access_User].[Roles]
                WHERE Estado = 1";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        // Recupera una página de roles activos con sus pantallas activas.
        // Pagina sobre Roles primero y luego JOIN con PantallasXRoles para evitar N+1.
        public async Task<List<RolResponse>> GetRolesPaginadosAsync(int pagina, int tamano)
        {
            int offset = (pagina - 1) * tamano;

            const string sqlRoles = @"
                SELECT RolID, NombreRol
                FROM [Carnet_Access_User].[Roles]
                WHERE Estado = 1
                ORDER BY FechaCreacion DESC
                OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY";

            const string sqlPantallas = @"
                SELECT RolID, PantallaID
                FROM [Carnet_Access_User].[PantallasXRoles]
                WHERE RolID IN @RolIds AND Estado = 1";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();

            var roles = (await connection.QueryAsync<Rol>(sqlRoles, new { Offset = offset, Tamano = tamano })).ToList();
            if (!roles.Any()) return new List<RolResponse>();

            var rolIds = roles.Select(r => r.RolID).ToList();
            var pantallas = (await connection.QueryAsync<PantallaXRol>(sqlPantallas, new { RolIds = rolIds })).ToList();

            return roles.Select(r => new RolResponse
            {
                RolID = r.RolID,
                NombreRol = r.NombreRol,
                Pantallas = pantallas.Where(p => p.RolID == r.RolID).Select(p => p.PantallaID).ToList()
            }).ToList();
        }

        // Inserta el rol en Roles y sus pantallas en PantallasXRoles dentro de una transacción.
        // El cliente provee el RolID como UNIQUEIDENTIFIER.
        public async Task CrearRolAsync(Guid rolId, string nombreRol, List<Guid> pantallaIds)
        {
            const string sqlRol = @"
                INSERT INTO [Carnet_Access_User].[Roles] (RolID, NombreRol)
                VALUES (@RolID, @NombreRol)";

            const string sqlPantalla = @"
                INSERT INTO [Carnet_Access_User].[PantallasXRoles] (RolID, PantallaID)
                VALUES (@RolID, @PantallaID)";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(sqlRol, new { RolID = rolId, NombreRol = nombreRol }, transaction);

                foreach (var pantallaId in pantallaIds)
                    await connection.ExecuteAsync(sqlPantalla, new { RolID = rolId, PantallaID = pantallaId }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Actualiza NombreRol en Roles y sincroniza PantallasXRoles dentro de una transacción.
        // Sincronización:
        //   - Pantallas en la nueva lista pero no en la actual → INSERT o reactivación (Estado=1)
        //   - Pantallas en ambas listas → se conservan sin cambio
        //   - Pantallas en la lista actual pero no en la nueva → soft delete (Estado=0)
        public async Task ActualizarRolAsync(Guid rolId, string nombreRol, List<Guid> pantallaIds)
        {
            const string sqlUpdateRol = @"
                UPDATE [Carnet_Access_User].[Roles]
                SET NombreRol = @NombreRol
                WHERE RolID = @RolID AND Estado = 1";

            // Soft delete de pantallas que ya no están en la nueva lista
            const string sqlSoftDeletePantallas = @"
                UPDATE [Carnet_Access_User].[PantallasXRoles]
                SET Estado = 0
                WHERE RolID = @RolID AND Estado = 1 AND PantallaID NOT IN @PantallaIds";

            // Upsert: activa si ya existe (cualquier Estado), inserta si es nueva
            const string sqlUpsertPantalla = @"
                IF EXISTS (
                    SELECT 1 FROM [Carnet_Access_User].[PantallasXRoles]
                    WHERE RolID = @RolID AND PantallaID = @PantallaID
                )
                    UPDATE [Carnet_Access_User].[PantallasXRoles]
                    SET Estado = 1
                    WHERE RolID = @RolID AND PantallaID = @PantallaID
                ELSE
                    INSERT INTO [Carnet_Access_User].[PantallasXRoles] (RolID, PantallaID)
                    VALUES (@RolID, @PantallaID)";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(sqlUpdateRol, new { RolID = rolId, NombreRol = nombreRol }, transaction);

                await connection.ExecuteAsync(sqlSoftDeletePantallas,
                    new { RolID = rolId, PantallaIds = pantallaIds }, transaction);

                foreach (var pantallaId in pantallaIds)
                    await connection.ExecuteAsync(sqlUpsertPantalla,
                        new { RolID = rolId, PantallaID = pantallaId }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Soft delete del rol y de todas sus PantallasXRoles dentro de una transacción.
        // No realiza DELETE físico porque los triggers lo bloquean.
        public async Task EliminarRolAsync(Guid rolId)
        {
            const string sqlPantallas = @"
                UPDATE [Carnet_Access_User].[PantallasXRoles]
                SET Estado = 0
                WHERE RolID = @RolID AND Estado = 1";

            const string sqlRol = @"
                UPDATE [Carnet_Access_User].[Roles]
                SET Estado = 0
                WHERE RolID = @RolID AND Estado = 1";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(sqlPantallas, new { RolID = rolId }, transaction);
                await connection.ExecuteAsync(sqlRol, new { RolID = rolId }, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}


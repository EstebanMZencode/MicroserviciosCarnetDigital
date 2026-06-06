using Dapper;
using MicroServicioBitacoras.Entities;

namespace MicroServicioBitacoras.Repository
{
    public class BitacoraRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public BitacoraRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        // Consultar todas las bitácoras (más recientes primero)
        public async Task<IEnumerable<Bitacora>> GetAllAsync()
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"SELECT BitacoraID, UsuarioID, Descripcion, FechaHora
                            FROM Bitacoras
                            ORDER BY FechaHora DESC";
                return await connection.QueryAsync<Bitacora>(sql);
            }
        }

        // Registrar una bitácora. La fecha la pone el servidor (SYSUTCDATETIME).
        // No se manda BitacoraID porque es IDENTITY (autoincremental).
        // Devuelve el ID generado por la base.
        public async Task<int> CreateAsync(Bitacora bitacora)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"INSERT INTO Bitacoras (UsuarioID, Descripcion, FechaHora)
                            VALUES (@UsuarioID, @Descripcion, SYSUTCDATETIME());
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                return await connection.ExecuteScalarAsync<int>(sql, bitacora);
            }
        }
    }
}

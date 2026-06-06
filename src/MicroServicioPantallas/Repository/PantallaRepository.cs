using Dapper;
using MicroServicioPantallas.Entities;

namespace MicroServicioPantallas.Repository
{
    public class PantallaRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public PantallaRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        // Obtener todas las pantallas
        public async Task<IEnumerable<Pantalla>> GetAllAsync()
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"SELECT PantallaID, NombrePantalla, Descripcion, Ruta,
                                   FechaCreacion, FechaModificacion, Estado
                            FROM Pantallas";
                return await connection.QueryAsync<Pantalla>(sql);
            }
        }

        // Obtener una pantalla por su llave primaria
        public async Task<Pantalla?> GetByIdAsync(int id)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"SELECT PantallaID, NombrePantalla, Descripcion, Ruta,
                                   FechaCreacion, FechaModificacion, Estado
                            FROM Pantallas
                            WHERE PantallaID = @id";
                return await connection.QueryFirstOrDefaultAsync<Pantalla>(sql, new { id });
            }
        }

        // Crear una pantalla. PantallaID es IDENTITY, no se manda.
        // Devuelve el ID generado por la base.
        public async Task<int> CreateAsync(Pantalla pantalla)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"INSERT INTO Pantallas (NombrePantalla, Descripcion, Ruta, FechaCreacion, Estado)
                            VALUES (@NombrePantalla, @Descripcion, @Ruta, SYSUTCDATETIME(), 1);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                return await connection.ExecuteScalarAsync<int>(sql, pantalla);
            }
        }

        // Modificar una pantalla
        public async Task<int> UpdateAsync(Pantalla pantalla)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"UPDATE Pantallas
                            SET NombrePantalla = @NombrePantalla,
                                Descripcion = @Descripcion,
                                Ruta = @Ruta,
                                FechaModificacion = SYSUTCDATETIME()
                            WHERE PantallaID = @PantallaID";
                return await connection.ExecuteAsync(sql, pantalla);
            }
        }

        // Eliminar una pantalla
        public async Task<int> DeleteAsync(int id)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = "DELETE FROM Pantallas WHERE PantallaID = @id";
                return await connection.ExecuteAsync(sql, new { id });
            }
        }
    }
}

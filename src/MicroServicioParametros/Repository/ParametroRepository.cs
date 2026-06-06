using Dapper;
using MicroServicioParametros.Entities;

namespace MicroServicioParametros.Repository
{
    public class ParametroRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ParametroRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        // Obtener todos los parámetros
        public async Task<IEnumerable<Parametro>> GetAllAsync()
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"SELECT Identificador, Valor, FechaCreacion, FechaModificacion, Estado
                            FROM Parametros";
                return await connection.QueryAsync<Parametro>(sql);
            }
        }

        // Obtener un parámetro por su llave primaria
        public async Task<Parametro?> GetByIdAsync(string id)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"SELECT Identificador, Valor, FechaCreacion, FechaModificacion, Estado
                            FROM Parametros
                            WHERE Identificador = @id";
                return await connection.QueryFirstOrDefaultAsync<Parametro>(sql, new { id });
            }
        }

        // Crear un parámetro
        public async Task<int> CreateAsync(Parametro parametro)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"INSERT INTO Parametros (Identificador, Valor, FechaCreacion, Estado)
                            VALUES (@Identificador, @Valor, SYSUTCDATETIME(), 1)";
                return await connection.ExecuteAsync(sql, parametro);
            }
        }

        // Modificar un parámetro
        public async Task<int> UpdateAsync(Parametro parametro)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"UPDATE Parametros
                            SET Valor = @Valor,
                                FechaModificacion = SYSUTCDATETIME()
                            WHERE Identificador = @Identificador";
                return await connection.ExecuteAsync(sql, parametro);
            }
        }

        // Eliminar un parámetro
        public async Task<int> DeleteAsync(string id)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = "DELETE FROM Parametros WHERE Identificador = @id";
                return await connection.ExecuteAsync(sql, new { id });
            }
        }
    }
}

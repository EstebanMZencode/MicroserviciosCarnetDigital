using System.Data;
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

        // Obtener paginado: el SP devuelve DOS result sets:
        //   1) las filas de la página
        //   2) el total de registros
        // Devolvemos ambos en una tupla.
        public async Task<(IEnumerable<Parametro> Items, int Total)> GetPaginadoAsync(
            int pageNumber, int pageSize, string? searchTerm,
            string sortColumn, string sortDirection, bool incluirEliminados)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var parameters = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    SortColumn = sortColumn,
                    SortDirection = sortDirection,
                    IncluirEliminados = incluirEliminados
                };

                using (var multi = await connection.QueryMultipleAsync(
                    "[Carnet_Config_User].[SP_Parametros_SelectPaginado]",
                    parameters,
                    commandType: CommandType.StoredProcedure))
                {
                    var items = await multi.ReadAsync<Parametro>();
                    var total = await multi.ReadFirstAsync<int>();
                    return (items, total);
                }
            }
        }

        // Obtener un parámetro por su llave primaria
        public async Task<Parametro?> GetByIdAsync(string id)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Parametro>(
                    "[Carnet_Config_User].[SP_Parametros_SelectByID]",
                    new { Identificador = id },
                    commandType: CommandType.StoredProcedure);
            }
        }

        // Crear: el SP inserta y devuelve el registro creado
        public async Task<Parametro?> CreateAsync(Parametro parametro)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Parametro>(
                    "[Carnet_Config_User].[SP_Parametros_Insert]",
                    new { parametro.Identificador, parametro.Valor },
                    commandType: CommandType.StoredProcedure);
            }
        }

        // Modificar: el SP recibe identificador, valor y estado
        public async Task<int> UpdateAsync(Parametro parametro)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.ExecuteAsync(
                    "[Carnet_Config_User].[SP_Parametros_Update]",
                    new { parametro.Identificador, parametro.Valor, parametro.Estado },
                    commandType: CommandType.StoredProcedure);
            }
        }

        // Eliminar lógico: el SP pone Estado = 0 (no borra físicamente)
        public async Task<int> LogicDeleteAsync(string id)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.ExecuteAsync(
                    "[Carnet_Config_User].[SP_Parametros_LogicDelete]",
                    new { Identificador = id },
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}
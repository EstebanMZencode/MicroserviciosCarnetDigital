using System.Data;
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

        public async Task<(IEnumerable<Pantalla> Items, int Total)> GetPaginadoAsync(
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
                    "[Carnet_Access_User].[SP_Pantallas_SelectPaginado]",
                    parameters,
                    commandType: CommandType.StoredProcedure))
                {
                    var items = await multi.ReadAsync<Pantalla>();
                    var total = await multi.ReadFirstAsync<int>();
                    return (items, total);
                }
            }
        }

        public async Task<Pantalla?> GetByIdAsync(Guid id)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Pantalla>(
                    "[Carnet_Access_User].[SP_Pantallas_SelectByID]",
                    new { PantallaID = id },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<Pantalla?> CreateAsync(Pantalla pantalla)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Pantalla>(
                    "[Carnet_Access_User].[SP_Pantallas_Insert]",
                    new { pantalla.NombrePantalla, pantalla.Descripcion, pantalla.Ruta },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<int> UpdateAsync(Pantalla pantalla)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.ExecuteAsync(
                    "[Carnet_Access_User].[SP_Pantallas_Update]",
                    new { pantalla.PantallaID, pantalla.NombrePantalla, pantalla.Descripcion, pantalla.Ruta, pantalla.Estado },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<int> LogicDeleteAsync(Guid id)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.ExecuteAsync(
                    "[Carnet_Access_User].[SP_Pantallas_LogicDelete]",
                    new { PantallaID = id },
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}
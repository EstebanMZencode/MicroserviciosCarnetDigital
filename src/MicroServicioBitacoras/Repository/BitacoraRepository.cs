using System.Data;
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

        // Consultar paginado: el SP devuelve DOS result sets (filas + total)
        public async Task<(IEnumerable<Bitacora> Items, int Total)> GetPaginadoAsync(
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
                    "[Carnet_Audit_User].[SP_Bitacoras_SelectPaginado]",
                    parameters,
                    commandType: CommandType.StoredProcedure))
                {
                    var items = await multi.ReadAsync<Bitacora>();
                    var total = await multi.ReadFirstAsync<int>();
                    return (items, total);
                }
            }
        }

        // Registrar una bitácora. El SP inserta y devuelve el registro creado.
        // BitacoraID (GUID) y FechaHora los pone la base.
        public async Task<Bitacora?> CreateAsync(Bitacora bitacora)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Bitacora>(
                    "[Carnet_Audit_User].[SP_Bitacoras_Insert]",
                    new { bitacora.UsuarioID, bitacora.Descripcion },
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}

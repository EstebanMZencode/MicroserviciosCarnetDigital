using MicroServicioActualizarEstadoUsuario.Entities;
using Dapper;
using System.Data;

namespace MicroServicioActualizarEstadoUsuario.Repository
{
    public class EstadoUsuarioRepository : IEstadoUsuarioRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public EstadoUsuarioRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<EstadoUsuarioResponse?> UpdateEstadoUsuarioAsync(
            string emailUsuario, Guid estadoId)
        {
            const string sql = "[Carnet_Identity_User].[SP_Estado_Usuario_Update]";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();

            var resultado = await connection.QueryFirstOrDefaultAsync<EstadoUsuarioResponse>(
                sql,
                new
                {
                    EmailUsuario = emailUsuario,
                    EstadoID = estadoId
                },
                commandType: CommandType.StoredProcedure
            );

            return resultado;
        }
    }
}
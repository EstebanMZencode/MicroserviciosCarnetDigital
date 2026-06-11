using Dapper;
using MicroServicioFotografias.Entities;
using System.Data;

namespace MicroServicioFotografias.Repository
{
    // Repositorio de fotografías. Ejecuta todos los queries SQL contra el schema
    // [Carnet_Identity_User] de la base tiusr23pl_Carnet_Identity usando Dapper.
    // No utiliza procedimientos almacenados.
    public class FotografiasRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public FotografiasRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        // Busca el UsuarioID y FotoBase64 actual haciendo JOIN entre EmailXUsuarios y Usuarios.
        // Retorna null si el email no existe o está inactivo en EmailXUsuarios.
        public async Task<UsuarioFoto?> GetUsuarioPorEmailAsync(string email)
        {
            const string sql = @"
                SELECT u.UsuarioID, u.FotoBase64
                FROM [Carnet_Identity_User].[EmailXUsuarios] exu
                INNER JOIN [Carnet_Identity_User].[Usuarios] u
                    ON exu.UsuarioID = u.UsuarioID
                WHERE exu.Email = @Email
                  AND exu.Estado = 1";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            return await connection.QueryFirstOrDefaultAsync<UsuarioFoto>(sql, new { Email = email });
        }

        // Actualiza la columna FotoBase64 de la tabla Usuarios para el UsuarioID indicado.
        // El trigger TRG_Usuarios_ProtegerUpdate actualiza FechaModificacion automáticamente.
        public async Task ActualizarFotografiaAsync(Guid usuarioId, string fotoBase64)
        {
            const string sql = @"
                UPDATE [Carnet_Identity_User].[Usuarios]
                SET FotoBase64 = @FotoBase64
                WHERE UsuarioID = @UsuarioID";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            await connection.ExecuteAsync(sql, new { UsuarioID = usuarioId, FotoBase64 = fotoBase64 });
        }

        // Establece FotoBase64 en NULL para el UsuarioID indicado.
        // El trigger TRG_Usuarios_ProtegerUpdate actualiza FechaModificacion automáticamente.
        public async Task EliminarFotografiaAsync(Guid usuarioId)
        {
            const string sql = @"
                UPDATE [Carnet_Identity_User].[Usuarios]
                SET FotoBase64 = NULL
                WHERE UsuarioID = @UsuarioID";

            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            await connection.ExecuteAsync(sql, new { UsuarioID = usuarioId });
        }
    }
}

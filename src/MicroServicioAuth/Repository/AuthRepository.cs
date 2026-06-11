using Dapper;
using MicroServicioAuth.Entities;
using System.Data;

namespace MicroServicioAuth.Repository
{
    /// <summary>
    /// Repositorio de autenticación. Ejecuta todos los queries SQL necesarios
    /// contra el schema <c>[Carnet_Identity_User]</c> de la base <c>Carnet_Identity</c>
    /// usando Dapper. No utiliza procedimientos almacenados.
    /// </summary>
    public class AuthRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public AuthRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        /// <summary>
        /// Obtiene los datos de autenticación del usuario validando que el email exista
        /// en Login con estado activo, y que el TipoUsuarioID proporcionado corresponda
        /// a un registro activo en UsuariosXInstituciones.
        /// Retorna <c>null</c> si el email no existe o el TipoUsuario no corresponde.
        /// </summary>
        /// <param name="email">Email del usuario (PK en Login).</param>
        /// <param name="tipoUsuarioId">TipoUsuarioID de UsuariosXInstituciones a validar.</param>
        public async Task<UserLoginData?> GetLoginDataAsync(string email, Guid tipoUsuarioId)
        {
            const string sql = @"
                SELECT TOP 1
                    u.UsuarioID,
                    l.Email,
                    l.PasswordHash,
                    uxi.TipoUsuarioID,
                    uxi.RolID
                FROM [Carnet_Identity_User].[Login] l
                INNER JOIN [Carnet_Identity_User].[EmailXUsuarios] exu
                    ON  l.Email    = exu.Email
                    AND exu.Estado = 1
                INNER JOIN [Carnet_Identity_User].[Usuarios] u
                    ON exu.UsuarioID = u.UsuarioID
                INNER JOIN [Carnet_Identity_User].[UsuariosXInstituciones] uxi
                    ON  u.UsuarioID      = uxi.UsuarioID
                    AND uxi.Estado       = 1
                    AND uxi.TipoUsuarioID = @TipoUsuarioID
                WHERE l.Email  = @Email
                  AND l.Estado = 1
                ORDER BY uxi.FechaCreacion DESC";

            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                connection.Open();
                return await connection.QueryFirstOrDefaultAsync<UserLoginData>(
                    sql, new { Email = email, TipoUsuarioID = tipoUsuarioId });
            }
        }

        /// <summary>
        /// Recupera el registro de RefreshToken únicamente si el TokenRefresh coincide
        /// con el almacenado y la FechaExpiracion aún no ha sido superada (UTC).
        /// Retorna <c>null</c> si el email no existe, el token no coincide o ya expiró.
        /// </summary>
        /// <param name="email">Email del usuario (PK en RefreshToken).</param>
        /// <param name="tokenRefresh">Token de refresco recibido en el header del request.</param>
        public async Task<RefreshTokenRecord?> GetActiveRefreshTokenAsync(string email, string tokenRefresh)
        {
            const string sql = @"
                SELECT Email, TokenRefresh, FechaExpiracion
                FROM [Carnet_Identity_User].[RefreshToken]
                WHERE Email        = @Email
                  AND TokenRefresh = @TokenRefresh
                  AND FechaExpiracion > SYSUTCDATETIME()";

            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                connection.Open();
                return await connection.QueryFirstOrDefaultAsync<RefreshTokenRecord>(
                    sql, new { Email = email, TokenRefresh = tokenRefresh });
            }
        }

        /// <summary>
        /// Inserta la fila de RefreshToken si el email no existe en la tabla,
        /// o actualiza <c>TokenRefresh</c> y <c>FechaExpiracion</c> si ya existe.
        /// Usa <c>MERGE</c> para garantizar el upsert de forma atómica.
        /// </summary>
        /// <param name="email">Email del usuario (PK).</param>
        /// <param name="tokenRefresh">Nuevo token de refresco generado.</param>
        /// <param name="fechaExpiracion">Fecha y hora UTC de vencimiento del refresh token.</param>
        public async Task UpsertRefreshTokenAsync(string email, string tokenRefresh, DateTime fechaExpiracion)
        {
            const string sql = @"
                MERGE [Carnet_Identity_User].[RefreshToken] AS target
                USING (SELECT @Email AS Email) AS source
                    ON target.Email = source.Email
                WHEN MATCHED THEN
                    UPDATE SET
                        TokenRefresh    = @TokenRefresh,
                        FechaExpiracion = @FechaExpiracion
                WHEN NOT MATCHED THEN
                    INSERT (Email, TokenRefresh, FechaExpiracion)
                    VALUES (@Email, @TokenRefresh, @FechaExpiracion);";

            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                connection.Open();
                await connection.ExecuteAsync(sql, new
                {
                    Email = email,
                    TokenRefresh = tokenRefresh,
                    FechaExpiracion = fechaExpiracion
                });
            }
        }
    }
}

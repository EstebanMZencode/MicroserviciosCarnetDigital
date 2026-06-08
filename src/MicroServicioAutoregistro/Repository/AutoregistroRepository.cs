using Dapper;
using MicroServicioAutoregistro.Entities;
using Microsoft.Data.SqlClient;

namespace MicroServicioAutoregistro.Repository;

public class AutoregistroRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AutoregistroRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM [Carnet_Identity_User].[EmailXUsuarios] WHERE Email = @Email AND Estado = 1",
            new { Email = email });
        return count > 0;
    }

    public async Task<IEnumerable<string>> GetDominiosInstitucionAsync(Guid institucionId, string coreConnectionString)
    {
        using var connection = new SqlConnection(coreConnectionString);
        return await connection.QueryAsync<string>(
            @"SELECT NombreDominio 
              FROM [Carnet_Core_User].[DominiosInstituciones]
              WHERE InstitucionID = @InstitucionID AND Estado = 1",
            new { InstitucionID = institucionId });
    }

    public async Task<Guid> CreateUsuarioAsync(UsuarioRegistro usuario, string passwordHash,
        string tokenConfirmacion, DateTime tokenExpiracion)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var usuarioId = Guid.NewGuid();

            // 1. Obtener EstadoID de PENDIENTE
            var estadoId = await connection.ExecuteScalarAsync<Guid>(
                "SELECT EstadoID FROM [Carnet_Identity_User].[EstadosUsuarios] WHERE NombreEstado = 'PENDIENTE'",
                transaction: transaction);

            // 2. Insertar Usuarios
            await connection.ExecuteAsync(
                @"INSERT INTO [Carnet_Identity_User].[Usuarios]
                    (UsuarioID, TipoIdentID, Identificacion, NombreCompleto, EstadoID)
                  VALUES
                    (@UsuarioID, @TipoIdentID, @Identificacion, @NombreCompleto, @EstadoID)",
                new
                {
                    UsuarioID = usuarioId,
                    usuario.TipoIdentID,
                    usuario.Identificacion,
                    usuario.NombreCompleto,
                    EstadoID = estadoId
                }, transaction);

            // 3. Insertar EmailXUsuarios
            await connection.ExecuteAsync(
                @"INSERT INTO [Carnet_Identity_User].[EmailXUsuarios]
                    (Email, UsuarioID, InstitucionID)
                  VALUES
                    (@Email, @UsuarioID, @InstitucionID)",
                new
                {
                    usuario.Email,
                    UsuarioID = usuarioId,
                    usuario.InstitucionID
                }, transaction);

            // 4. Insertar Login con contraseña hasheada y token de confirmación en PasswordHash temporalmente
            await connection.ExecuteAsync(
                @"INSERT INTO [Carnet_Identity_User].[Login]
                    (Email, PasswordHash)
                  VALUES
                    (@Email, @PasswordHash)",
                new
                {
                    usuario.Email,
                    PasswordHash = $"{passwordHash}|TOKEN:{tokenConfirmacion}|EXP:{tokenExpiracion:O}"
                }, transaction);

            // 5. Insertar UsuariosXInstituciones
            var uxiId = Guid.NewGuid();
            await connection.ExecuteAsync(
                @"INSERT INTO [Carnet_Identity_User].[UsuariosXInstituciones]
                    (UXIID, UsuarioID, InstitucionID, TipoUsuarioID, RolID, FechaVencimientoCarnet)
                  VALUES
                    (@UXIID, @UsuarioID, @InstitucionID, @TipoUsuarioID, @RolID, @FechaVencimientoCarnet)",
                new
                {
                    UXIID = uxiId,
                    UsuarioID = usuarioId,
                    usuario.InstitucionID,
                    usuario.TipoUsuarioID,
                    usuario.RolID,
                    usuario.FechaVencimientoCarnet
                }, transaction);

            // 6. Insertar carreras
            foreach (var carreraId in usuario.CarrerasIDs)
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO [Carnet_Identity_User].[UsuariosXCarreras]
                        (UXC_ID, UXIID, CarreraID)
                      VALUES
                        (NEWID(), @UXIID, @CarreraID)",
                    new { UXIID = uxiId, CarreraID = carreraId }, transaction);
            }

            // 7. Insertar áreas de trabajo
            foreach (var areaId in usuario.AreasTrabajoIDs)
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO [Carnet_Identity_User].[UsuariosXAreasTrabajo]
                        (UXAT_ID, UXIID, AreaTrabID)
                      VALUES
                        (NEWID(), @UXIID, @AreaTrabID)",
                    new { UXIID = uxiId, AreaTrabID = areaId }, transaction);
            }

            // 8. Insertar teléfonos (opcionales)
            foreach (var telefono in usuario.Telefonos)
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO [Carnet_Identity_User].[TelefonosUsuarios]
                        (TelID, UsuarioID, Telefono)
                      VALUES
                        (NEWID(), @UsuarioID, @Telefono)",
                    new { UsuarioID = usuarioId, Telefono = telefono }, transaction);
            }

            await transaction.CommitAsync();
            return usuarioId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(bool Exists, bool Expired, string Email)> GetTokenDataAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();

        // El token está almacenado en PasswordHash en formato: "hash|TOKEN:xxx|EXP:fecha"
        var logins = await connection.QueryAsync<dynamic>(
            @"SELECT L.Email, L.PasswordHash
              FROM [Carnet_Identity_User].[Login] L
              INNER JOIN [Carnet_Identity_User].[Usuarios] U 
                ON L.Email = (SELECT Email FROM [Carnet_Identity_User].[EmailXUsuarios] WHERE UsuarioID = U.UsuarioID)
              INNER JOIN [Carnet_Identity_User].[EstadosUsuarios] E 
                ON U.EstadoID = E.EstadoID
              WHERE E.NombreEstado = 'PENDIENTE'
                AND L.PasswordHash LIKE @TokenPattern",
            new { TokenPattern = $"%TOKEN:{token}%" });

        var login = logins.FirstOrDefault();
        if (login is null) return (false, false, string.Empty);

        // Extraer fecha de expiración del PasswordHash
        string passwordHash = login.PasswordHash;
        var expPart = passwordHash.Split("|EXP:").LastOrDefault();
        if (expPart is null || !DateTime.TryParse(expPart, out var expiracion))
            return (true, true, string.Empty);

        bool expired = DateTime.UtcNow > expiracion;
        return (true, expired, (string)login.Email);
    }

    public async Task ConfirmarUsuarioAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Obtener EstadoID de ACTIVO
            var estadoActivoId = await connection.ExecuteScalarAsync<Guid>(
                "SELECT EstadoID FROM [Carnet_Identity_User].[EstadosUsuarios] WHERE NombreEstado = 'ACTIVO'",
                transaction: transaction);

            // Obtener UsuarioID
            var usuarioId = await connection.ExecuteScalarAsync<Guid>(
                "SELECT UsuarioID FROM [Carnet_Identity_User].[EmailXUsuarios] WHERE Email = @Email",
                new { Email = email }, transaction);

            // Actualizar estado del usuario
            await connection.ExecuteAsync(
                @"UPDATE [Carnet_Identity_User].[Usuarios]
                  SET EstadoID = @EstadoID, FechaModificacion = SYSUTCDATETIME()
                  WHERE UsuarioID = @UsuarioID",
                new { EstadoID = estadoActivoId, UsuarioID = usuarioId }, transaction);

            // Limpiar token del PasswordHash — dejar solo el hash real
            var passwordHash = await connection.ExecuteScalarAsync<string>(
                "SELECT PasswordHash FROM [Carnet_Identity_User].[Login] WHERE Email = @Email",
                new { Email = email }, transaction);

            var hashLimpio = passwordHash?.Split("|TOKEN:").FirstOrDefault() ?? string.Empty;

            await connection.ExecuteAsync(
                @"UPDATE [Carnet_Identity_User].[Login]
                  SET PasswordHash = @PasswordHash, FechaModificacion = SYSUTCDATETIME()
                  WHERE Email = @Email",
                new { PasswordHash = hashLimpio, Email = email }, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
using Dapper;
using MicroServicioAutoregistro.Entities;

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
            "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email",
            new { Email = email });
        return count > 0;
    }

    // Obtiene los dominios de la institución desde tiusr23pl_Carnet_Core
    // Nota: como es otra BD, se usa un connection string separado (CoreConnection)
    public async Task<IEnumerable<string>> GetDominiosInstitucionAsync(int institucionId, string coreConnectionString)
    {
        using var connection = new MySqlConnector.MySqlConnection(coreConnectionString);
        var dominios = await connection.QueryAsync<string>(
            "SELECT Dominio FROM DominiosInstituciones WHERE InstitucionId = @Id",
            new { Id = institucionId });
        return dominios;
    }

    public async Task<int> CreateUsuarioAsync(UsuarioRegistro usuario, string contrasenaHash, string tokenConfirmacion, DateTime tokenExpiracion)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Insertar en tabla Usuarios
            var userId = await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Usuarios 
                    (Email, TipoIdentificacionId, Identificacion, NombreCompleto, InstitucionId, 
                     Contrasena, TipoUsuarioId, RolId, Estado, TokenConfirmacion, TokenExpiracion)
                  VALUES 
                    (@Email, @TipoIdentificacionId, @Identificacion, @NombreCompleto, @InstitucionId,
                     @Contrasena, @TipoUsuarioId, @RolId, 'PENDIENTE', @Token, @Expiracion);
                  SELECT LAST_INSERT_ID();",
                new
                {
                    usuario.Email,
                    usuario.TipoIdentificacionId,
                    usuario.Identificacion,
                    usuario.NombreCompleto,
                    usuario.InstitucionId,
                    Contrasena = contrasenaHash,
                    usuario.TipoUsuarioId,
                    usuario.RolId,
                    Token = tokenConfirmacion,
                    Expiracion = tokenExpiracion
                }, transaction);

            // Insertar en EmailXUsuarios
            await connection.ExecuteAsync(
                "INSERT INTO EmailXUsuarios (UsuarioId, Email) VALUES (@UsuarioId, @Email)",
                new { UsuarioId = userId, usuario.Email }, transaction);

            // Insertar en EstadosUsuarios
            await connection.ExecuteAsync(
                "INSERT INTO EstadosUsuarios (UsuarioId, Estado) VALUES (@UsuarioId, 'PENDIENTE')",
                new { UsuarioId = userId }, transaction);

            // Insertar en UsuariosXInstituciones
            await connection.ExecuteAsync(
                "INSERT INTO UsuariosXInstituciones (UsuarioId, InstitucionId) VALUES (@UsuarioId, @InstitucionId)",
                new { UsuarioId = userId, usuario.InstitucionId }, transaction);

            // Carreras (si es estudiante)
            foreach (var carreraId in usuario.CarrerasIds)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO UsuariosXCarreras (UsuarioId, CarreraId) VALUES (@UsuarioId, @CarreraId)",
                    new { UsuarioId = userId, CarreraId = carreraId }, transaction);
            }

            // Áreas (si es funcionario)
            foreach (var areaId in usuario.AreasIds)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO UsuariosXAreasTrabajo (UsuarioId, AreaId) VALUES (@UsuarioId, @AreaId)",
                    new { UsuarioId = userId, AreaId = areaId }, transaction);
            }

            // Teléfonos (opcionales)
            foreach (var telefono in usuario.Telefonos)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO TelefonosUsuarios (UsuarioId, Telefono) VALUES (@UsuarioId, @Telefono)",
                    new { UsuarioId = userId, Telefono = telefono }, transaction);
            }

            await transaction.CommitAsync();
            return userId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(bool Exists, bool Expired, int UsuarioId)> GetTokenDataAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync(
            "SELECT Id, TokenExpiracion FROM Usuarios WHERE TokenConfirmacion = @Token AND Estado = 'PENDIENTE'",
            new { Token = token });

        if (result is null)
            return (false, false, 0);

        bool expired = result.TokenExpiracion < DateTime.UtcNow;
        return (true, expired, (int)result.Id);
    }

    public async Task ConfirmarUsuarioAsync(int usuarioId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync(
                "UPDATE Usuarios SET Estado = 'ACTIVO', TokenConfirmacion = NULL, TokenExpiracion = NULL WHERE Id = @Id",
                new { Id = usuarioId }, transaction);

            await connection.ExecuteAsync(
                "UPDATE EstadosUsuarios SET Estado = 'ACTIVO' WHERE UsuarioId = @Id",
                new { Id = usuarioId }, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
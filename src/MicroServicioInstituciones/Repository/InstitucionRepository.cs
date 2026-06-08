using Dapper;
using MicroServicioInstituciones.Entities;

namespace MicroServicioInstituciones.Repository;

public class InstitucionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InstitucionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Institucion>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var instituciones = (await connection.QueryAsync<Institucion>(
            @"SELECT InstitucionID, NombreInstitucion, Email, Telefono, Estado, 
                     FechaCreacion, FechaModificacion
              FROM [Carnet_Core_User].[Instituciones]
              WHERE Estado = 1")).ToList();

        foreach (var inst in instituciones)
        {
            inst.Dominios = (await GetDominiosAsync(connection, inst.InstitucionID)).ToList();
        }

        return instituciones;
    }

    public async Task<Institucion?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var inst = await connection.QueryFirstOrDefaultAsync<Institucion>(
            @"SELECT InstitucionID, NombreInstitucion, Email, Telefono, Estado,
                     FechaCreacion, FechaModificacion
              FROM [Carnet_Core_User].[Instituciones]
              WHERE InstitucionID = @InstitucionID AND Estado = 1",
            new { InstitucionID = id });

        if (inst is null) return null;

        inst.Dominios = (await GetDominiosAsync(connection, inst.InstitucionID)).ToList();
        return inst;
    }

    public async Task<Guid> CreateAsync(InstitucionRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var newId = Guid.NewGuid();

            await connection.ExecuteAsync(
                @"INSERT INTO [Carnet_Core_User].[Instituciones] 
                    (InstitucionID, NombreInstitucion, Email, Telefono)
                  VALUES 
                    (@InstitucionID, @NombreInstitucion, @Email, @Telefono)",
                new
                {
                    InstitucionID = newId,
                    request.NombreInstitucion,
                    request.Email,
                    request.Telefono
                }, transaction);

            foreach (var dominio in request.Dominios)
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO [Carnet_Core_User].[DominiosInstitucion]
                        (DominioID, InstitucionID, NombreDominio)
                      VALUES 
                        (NEWID(), @InstitucionID, @NombreDominio)",
                    new { InstitucionID = newId, NombreDominio = dominio }, transaction);
            }

            await transaction.CommitAsync();
            return newId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Guid id, InstitucionRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var rows = await connection.ExecuteAsync(
                @"UPDATE [Carnet_Core_User].[Instituciones]
                  SET NombreInstitucion = @NombreInstitucion,
                      Email = @Email,
                      Telefono = @Telefono,
                      FechaModificacion = SYSUTCDATETIME()
                  WHERE InstitucionID = @InstitucionID AND Estado = 1",
                new
                {
                    InstitucionID = id,
                    request.NombreInstitucion,
                    request.Email,
                    request.Telefono
                }, transaction);

            if (rows == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Soft delete dominios anteriores y reinsertar
            await connection.ExecuteAsync(
                @"UPDATE [Carnet_Core_User].[DominiosInstitucion]
                  SET Estado = 0, FechaModificacion = SYSUTCDATETIME()
                  WHERE InstitucionID = @InstitucionID",
                new { InstitucionID = id }, transaction);

            foreach (var dominio in request.Dominios)
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO [Carnet_Core_User].[DominiosInstitucion]
                        (DominioID, InstitucionID, NombreDominio)
                      VALUES 
                        (NEWID(), @InstitucionID, @NombreDominio)",
                    new { InstitucionID = id, NombreDominio = dominio }, transaction);
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Soft delete dominios
            await connection.ExecuteAsync(
                @"UPDATE [Carnet_Core_User].[DominiosInstitucion]
                  SET Estado = 0, FechaModificacion = SYSUTCDATETIME()
                  WHERE InstitucionID = @InstitucionID",
                new { InstitucionID = id }, transaction);

            // Soft delete institución
            var rows = await connection.ExecuteAsync(
                @"UPDATE [Carnet_Core_User].[Instituciones]
                  SET Estado = 0, FechaModificacion = SYSUTCDATETIME()
                  WHERE InstitucionID = @InstitucionID AND Estado = 1",
                new { InstitucionID = id }, transaction);

            await transaction.CommitAsync();
            return rows > 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task<IEnumerable<string>> GetDominiosAsync(
        Microsoft.Data.SqlClient.SqlConnection connection, Guid institucionId)
    {
        var dominios = await connection.QueryAsync<string>(
            @"SELECT NombreDominio 
              FROM [Carnet_Core_User].[DominiosInstitucion]
              WHERE InstitucionID = @InstitucionID AND Estado = 1",
            new { InstitucionID = institucionId });
        return dominios;
    }
}
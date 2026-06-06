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

        // TODO: ajustar nombres de columnas cuando se confirme estructura real
        var instituciones = (await connection.QueryAsync<Institucion>(
            "SELECT Id, Nombre, Email, Telefono FROM Instituciones")).ToList();

        foreach (var inst in instituciones)
        {
            inst.Dominios = (await GetDominiosAsync(connection, inst.Id)).ToList();
        }

        return instituciones;
    }

    public async Task<Institucion?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var inst = await connection.QueryFirstOrDefaultAsync<Institucion>(
            "SELECT Id, Nombre, Email, Telefono FROM Instituciones WHERE Id = @Id",
            new { Id = id });

        if (inst is null) return null;

        inst.Dominios = (await GetDominiosAsync(connection, inst.Id)).ToList();
        return inst;
    }

    public async Task<int> CreateAsync(InstitucionRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var id = await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Instituciones (Nombre, Email, Telefono)
                  VALUES (@Nombre, @Email, @Telefono);
                  SELECT LAST_INSERT_ID();",
                request, transaction);

            foreach (var dominio in request.Dominios)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO DominiosInstituciones (InstitucionId, Dominio) VALUES (@InstitucionId, @Dominio)",
                    new { InstitucionId = id, Dominio = dominio }, transaction);
            }

            await transaction.CommitAsync();
            return id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, InstitucionRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var rows = await connection.ExecuteAsync(
                @"UPDATE Instituciones
                  SET Nombre = @Nombre, Email = @Email, Telefono = @Telefono
                  WHERE Id = @Id",
                new { request.Nombre, request.Email, request.Telefono, Id = id },
                transaction);

            if (rows == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Reemplazar dominios
            await connection.ExecuteAsync(
                "DELETE FROM DominiosInstituciones WHERE InstitucionId = @Id",
                new { Id = id }, transaction);

            foreach (var dominio in request.Dominios)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO DominiosInstituciones (InstitucionId, Dominio) VALUES (@InstitucionId, @Dominio)",
                    new { InstitucionId = id, Dominio = dominio }, transaction);
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

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Primero borrar dominios (FK)
            await connection.ExecuteAsync(
                "DELETE FROM DominiosInstituciones WHERE InstitucionId = @Id",
                new { Id = id }, transaction);

            var rows = await connection.ExecuteAsync(
                "DELETE FROM Instituciones WHERE Id = @Id",
                new { Id = id }, transaction);

            await transaction.CommitAsync();
            return rows > 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Helper privado — reutiliza la conexión abierta
    private static async Task<IEnumerable<string>> GetDominiosAsync(MySqlConnector.MySqlConnection connection, int institucionId)
    {
        var dominios = await connection.QueryAsync<DominioInstitucion>(
            "SELECT Id, InstitucionId, Dominio FROM DominiosInstituciones WHERE InstitucionId = @Id",
            new { Id = institucionId });
        return dominios.Select(d => d.Dominio);
    }
}
using Dapper;
using MicroServicioTiposID.Entities;

namespace MicroServicioTiposID.Repository;

public class TipoIdentificacionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TipoIdentificacionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<TipoIdentificacion>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<TipoIdentificacion>(
            "SELECT Id, Nombre FROM TiposIdentificaciones");
    }

    public async Task<TipoIdentificacion?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<TipoIdentificacion>(
            "SELECT Id, Nombre FROM TiposIdentificaciones WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<int> CreateAsync(TipoIdentificacion tipo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO TiposIdentificaciones (Nombre) VALUES (@Nombre);
              SELECT LAST_INSERT_ID();",
            tipo);
    }

    public async Task<bool> UpdateAsync(TipoIdentificacion tipo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            "UPDATE TiposIdentificaciones SET Nombre = @Nombre WHERE Id = @Id",
            tipo);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            "DELETE FROM TiposIdentificaciones WHERE Id = @Id",
            new { Id = id });
        return rows > 0;
    }
}
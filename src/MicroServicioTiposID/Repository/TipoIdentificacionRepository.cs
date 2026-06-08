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
            @"SELECT TipoIdentID, NombreTipoIdent, Estado, FechaCreacion, FechaModificacion
              FROM [Carnet_Identity_User].[TiposIdentificaciones]
              WHERE Estado = 1");
    }

    public async Task<TipoIdentificacion?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<TipoIdentificacion>(
            @"SELECT TipoIdentID, NombreTipoIdent, Estado, FechaCreacion, FechaModificacion
              FROM [Carnet_Identity_User].[TiposIdentificaciones]
              WHERE TipoIdentID = @TipoIdentID AND Estado = 1",
            new { TipoIdentID = id });
    }

    public async Task<Guid> CreateAsync(TipoIdentificacionRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        var newId = Guid.NewGuid();

        await connection.ExecuteAsync(
            @"INSERT INTO [Carnet_Identity_User].[TiposIdentificaciones]
                (TipoIdentID, NombreTipoIdent)
              VALUES
                (@TipoIdentID, @NombreTipoIdent)",
            new { TipoIdentID = newId, request.NombreTipoIdent });

        return newId;
    }

    public async Task<bool> UpdateAsync(Guid id, TipoIdentificacionRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            @"UPDATE [Carnet_Identity_User].[TiposIdentificaciones]
              SET NombreTipoIdent = @NombreTipoIdent,
                  FechaModificacion = SYSUTCDATETIME()
              WHERE TipoIdentID = @TipoIdentID AND Estado = 1",
            new { TipoIdentID = id, request.NombreTipoIdent });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            @"UPDATE [Carnet_Identity_User].[TiposIdentificaciones]
              SET Estado = 0, FechaModificacion = SYSUTCDATETIME()
              WHERE TipoIdentID = @TipoIdentID AND Estado = 1",
            new { TipoIdentID = id });
        return rows > 0;
    }
}
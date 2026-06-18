using Dapper;
using MicroServicioAuth.Repository;
using MicroServicioUsuarios.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MicroServicioUsuarios.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<UsuarioRepository> _logger;

        public UsuarioRepository(IDbConnectionFactory dbConnectionFactory, ILogger<UsuarioRepository> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<(Guid usuarioID, List<string> emailsCreados)> CrearUsuarioCompletoAsync(
            UsuarioCreateRequest request,
            string passwordHash)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            var emailsCreados = new List<string>();

            try
            {
                // Insertar usuario base
                _logger.LogInformation("Insertando usuario: {Identificacion}", request.Identificacion);
                var usuarioID = await InsertUsuarioAsync(transaction, request);
                _logger.LogInformation("Usuario creado: {UsuarioID}", usuarioID);

                // Insertar emails y logins
                foreach (var emailReq in request.Emails)
                {
                    _logger.LogInformation("Insertando email: {Email}", emailReq.Email);
                    await InsertEmailAsync(transaction, emailReq.Email, usuarioID, emailReq.InstitucionID);
                    _logger.LogInformation("Insertando login: {Email}", emailReq.Email);
                    await InsertLoginAsync(transaction, emailReq.Email, passwordHash);
                    emailsCreados.Add(emailReq.Email);
                }

                // Insertar teléfonos (opcionales)
                foreach (var telefono in request.Telefonos)
                {
                    _logger.LogInformation("Insertando teléfono: {Telefono}", telefono);
                    await InsertTelefonoAsync(transaction, usuarioID, telefono);
                }

                // Insertar instituciones (UXI), carreras y áreas
                foreach (var inst in request.Instituciones)
                {
                    _logger.LogInformation("Insertando UXI: Institucion={InstitucionID}, TipoUsuario={TipoUsuarioID}, Rol={RolID}",
                        inst.InstitucionID, inst.TipoUsuarioID, inst.RolID);

                    var uxiID = await InsertUXIAsync(transaction, usuarioID, inst);
                    _logger.LogInformation("UXI creado: {UXIID}", uxiID);

                    foreach (var carreraID in inst.Carreras)
                    {
                        _logger.LogInformation("Insertando carrera: {CarreraID} en UXI {UXIID}", carreraID, uxiID);
                        await InsertCarreraAsync(transaction, uxiID, carreraID);
                    }

                    foreach (var areaID in inst.Areas)
                    {
                        _logger.LogInformation("Insertando área: {AreaID} en UXI {UXIID}", areaID, uxiID);
                        await InsertAreaAsync(transaction, uxiID, areaID);
                    }
                }

                transaction.Commit();
                _logger.LogInformation("Transacción COMMIT exitosa");
                return (usuarioID, emailsCreados);
            }
            catch (SqlException ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "SQL ERROR {Number}: {Message} — ROLLBACK ejecutado", ex.Number, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "ERROR inesperado: {Message} — ROLLBACK ejecutado", ex.Message);
                throw;
            }
        }

        // --- Métodos privados ------------------------------------------------

        private static async Task<Guid> InsertUsuarioAsync(IDbTransaction transaction, UsuarioCreateRequest request)
        {
            const string sql = "[Carnet_Identity_User].[SP_Usuarios_Insert]";
            var resultado = await transaction.Connection!.QueryFirstAsync<dynamic>(sql,
                new
                {
                    TipoIdentID = request.TipoIdentID,
                    Identificacion = request.Identificacion,
                    NombreCompleto = request.NombreCompleto,
                    EstadoID = request.EstadoID
                },
                transaction: transaction,
                commandType: CommandType.StoredProcedure);
            return (Guid)resultado.UsuarioID;
        }

        private static async Task InsertEmailAsync(IDbTransaction transaction, string email, Guid usuarioID, Guid institucionID)
        {
            const string sql = "[Carnet_Identity_User].[SP_Emails_Insert]";
            await transaction.Connection!.ExecuteAsync(sql,
                new { Email = email, UsuarioID = usuarioID, InstitucionID = institucionID },
                transaction: transaction,
                commandType: CommandType.StoredProcedure);
        }

        private static async Task InsertLoginAsync(IDbTransaction transaction, string email, string passwordHash)
        {
            const string sql = "[Carnet_Identity_User].[SP_Login_Insert]";
            await transaction.Connection!.ExecuteAsync(sql,
                new { Email = email, PasswordHash = passwordHash },
                transaction: transaction,
                commandType: CommandType.StoredProcedure);
        }

        private static async Task InsertTelefonoAsync(IDbTransaction transaction, Guid usuarioID, string telefono)
        {
            const string sql = "[Carnet_Identity_User].[SP_Telefonos_Insert]";
            await transaction.Connection!.ExecuteAsync(sql,
                new { UsuarioID = usuarioID, Telefono = telefono },
                transaction: transaction,
                commandType: CommandType.StoredProcedure);
        }

        private static async Task<Guid> InsertUXIAsync(IDbTransaction transaction, Guid usuarioID, InstitucionRequest inst)
        {
            const string sql = "[Carnet_Identity_User].[SP_UXI_Insert]";
            var resultado = await transaction.Connection!.QueryFirstAsync<dynamic>(sql,
                new
                {
                    UsuarioID = usuarioID,
                    InstitucionID = inst.InstitucionID,
                    TipoUsuarioID = inst.TipoUsuarioID,
                    RolID = inst.RolID,
                    FechaVencimientoCarnet = inst.FechaVencimientoCarnet
                },
                transaction: transaction,
                commandType: CommandType.StoredProcedure);
            return (Guid)resultado.UXIID;
        }

        private static async Task InsertCarreraAsync(IDbTransaction transaction, Guid uxiID, Guid carreraID)
        {
            const string sql = "[Carnet_Identity_User].[SP_UXCarreras_Insert]";
            await transaction.Connection!.ExecuteAsync(sql,
                new { UXIID = uxiID, CarreraID = carreraID },
                transaction: transaction,
                commandType: CommandType.StoredProcedure);
        }

        private static async Task InsertAreaAsync(IDbTransaction transaction, Guid uxiID, Guid areaID)
        {
            const string sql = "[Carnet_Identity_User].[SP_UXAreas_Insert]";
            await transaction.Connection!.ExecuteAsync(sql,
                new { UXIID = uxiID, AreaTrabID = areaID }, 
                transaction: transaction,
                commandType: CommandType.StoredProcedure);
        }
    }
}
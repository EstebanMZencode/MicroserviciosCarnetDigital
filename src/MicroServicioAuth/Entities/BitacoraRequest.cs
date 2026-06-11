namespace MicroServicioAuth.Entities
{
    /// <summary>
    /// Cuerpo del <c>POST</c> al endpoint <c>/bitacora</c> del <c>MicroServicioBitacoras</c>.
    /// Registra acciones realizadas por el sistema de autenticación en la base <c>Carnet_Audit</c>.
    /// </summary>
    public class BitacoraRequest
    {
        /// <summary>Email del usuario que ejecutó la acción (identificador en el sistema).</summary>
        public string usuarioID { get; set; } = string.Empty;

        /// <summary>Descripción legible de la acción registrada (máx. 255 chars según tabla Bitacoras).</summary>
        public string descripcion { get; set; } = string.Empty;
    }
}

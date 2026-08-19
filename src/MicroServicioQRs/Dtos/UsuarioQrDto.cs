namespace MicroServicioQRs.Dtos
{
    // Este es EXACTAMENTE el JSON que viaja dentro del QR.
    // GRD3 (app del guarda) escanea, lee este JSON, consulta el usuario
    // por su llave primaria (UsuarioID) y compara dato por dato contra la BD.
    // Elegimos solo lo identificador: robusto y sin JOINs.
    public class UsuarioQrDto
    {
        public Guid UsuarioID { get; set; }
        public string Identificacion { get; set; }
        public string NombreCompleto { get; set; }
    }
}
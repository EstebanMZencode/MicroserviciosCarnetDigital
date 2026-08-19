namespace MicroServicioQRs.Repository
{
    public interface IBitacoraRepository
    {
        // Registra una accion en la bitacora (fecha, usuario, descripcion).
        // Segun el documento, cada consulta/accion importante debe registrarse.
        Task Registrar(string usuario, string descripcion);
    }
}
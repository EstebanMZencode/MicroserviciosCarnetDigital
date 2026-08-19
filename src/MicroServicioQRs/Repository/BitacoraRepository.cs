using Microsoft.EntityFrameworkCore;

namespace MicroServicioQRs.Repository
{
    public class BitacoraRepository : IBitacoraRepository
    {
        private readonly QrDbContext _context;

        public BitacoraRepository(QrDbContext context)
        {
            _context = context;
        }

        public async Task Registrar(string usuario, string descripcion)
        {
            // DEFENSIVO: si la tabla Bitacoras tiene otro nombre/columnas o no existe,
            // el registro de bitacora falla en silencio y NO rompe la operacion del QR.
            // (Confirmar la tabla real y quitar este try/catch cuando este verificada.)
            try
            {
                var id = Guid.NewGuid();
                var fecha = DateTime.UtcNow;

                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"INSERT INTO [Carnet_Identity_User].[Bitacoras]
                       ([BitacoraID], [Fecha], [Usuario], [Descripcion])
                       VALUES ({id}, {fecha}, {usuario}, {descripcion})"
                );
            }
            catch (Exception ex)
            {
                // Se traga el error a proposito para no tumbar el QR por un fallo de bitacora.
                Console.WriteLine($"[Bitacora] No se pudo registrar: {ex.Message}");
            }
        }
    }
}
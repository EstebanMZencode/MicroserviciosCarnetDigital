using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioBitacoras;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloBitacoras
{
    public class BitacorasModel : PageModel
    {
        private readonly IBitacorasApiClient _api;
        public List<BitacoraDto> Bitacoras { get; set; } = new();
        public string? MensajeError { get; set; }
        public string? FiltroFecha { get; set; }
        public string? FiltroUsuario { get; set; }
        public string? FiltroAccion { get; set; }

        public BitacorasModel(IBitacorasApiClient api) => _api = api;

        private void AplicarToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _api.SetToken(token);
        }

        public async Task OnGetAsync(string? fecha = null, string? usuario = null, string? accion = null)
        {
            FiltroFecha = fecha;
            FiltroUsuario = usuario;
            FiltroAccion = accion;
            ViewData["Section"] = "Registro de Bitácoras";
            ViewData["UserName"] = Request.Cookies["UserName"] ?? "Usuario de prueba";
            ViewData["UserRole"] = Request.Cookies["UserRole"] ?? "";
            ViewData["UserInitials"] = "U";
            try
            {
                AplicarToken();
                var result = await _api.GetAllAsync(1, 100);
                var items = result.Items;

                if (!string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out var fechaFiltro))
                    items = items.Where(b => b.FechaHora?.Date == fechaFiltro.Date).ToList();

                if (!string.IsNullOrEmpty(usuario))
                    items = items.Where(b => b.UsuarioID.ToString().Contains(usuario, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrEmpty(accion))
                    items = items.Where(b => b.Descripcion.Contains(accion, StringComparison.OrdinalIgnoreCase)).ToList();

                Bitacoras = items.OrderByDescending(b => b.FechaHora).ToList();
            }
            catch (Exception ex) { MensajeError = $"Error al cargar: {ex.Message}"; }
        }
    }
}
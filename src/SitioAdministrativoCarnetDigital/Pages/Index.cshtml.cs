using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SitioAdministrativoCarnetDigital.Pages
{
    public class IndexModel : PageModel
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Saludo { get; set; } = string.Empty;
        public string FechaHoy { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            var token = Request.Cookies["JWToken"];

            // ============================================================
            // BYPASS TEMPORAL — comentado para poder ver el diseño mientras
            // no existe el login. REACTIVAR este bloque en cuanto el login
            // definitivo esté listo (descomentar).
            // ============================================================
            // if (string.IsNullOrEmpty(token))
            // {
            //     TempData["MensajeLogin"] = "Por favor inicie sesión para utilizar el sistema";
            //     return RedirectToPage("/Login");
            // }

            NombreUsuario = Request.Cookies["UserName"] ?? "Usuario de prueba";
            Saludo = ObtenerSaludo(DateTime.Now.Hour);

            var cultura = new System.Globalization.CultureInfo("es-CR");
            FechaHoy = DateTime.Now.ToString("dddd d 'de' MMMM 'de' yyyy", cultura);
            FechaHoy = char.ToUpper(FechaHoy[0]) + FechaHoy.Substring(1);

            ViewData["Section"] = "Inicio";
            ViewData["UserName"] = NombreUsuario;
            ViewData["UserRole"] = Request.Cookies["UserRole"] ?? "";
            ViewData["UserInitials"] = ObtenerIniciales(NombreUsuario);

            return Page();
        }

        private static string ObtenerSaludo(int hora)
        {
            if (hora < 12) return "Buenos días";
            if (hora < 19) return "Buenas tardes";
            return "Buenas noches";
        }

        private static string ObtenerIniciales(string nombreCompleto)
        {
            var partes = nombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return "U";
            if (partes.Length == 1) return partes[0][0].ToString().ToUpper();
            return $"{partes[0][0]}{partes[^1][0]}".ToUpper();
        }
    }
}
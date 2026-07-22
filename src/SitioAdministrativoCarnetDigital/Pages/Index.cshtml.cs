using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SitioAdministrativoCarnetDigital.Pages
{
    public class IndexModel : PageModel
    {
        public string NombreUsuario { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            var token = Request.Cookies["JWToken"];

            
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

            ViewData["Section"] = "Inicio";
            ViewData["UserName"] = NombreUsuario;
            ViewData["UserRole"] = Request.Cookies["UserRole"] ?? "";
            ViewData["UserInitials"] = ObtenerIniciales(NombreUsuario);

            return Page();
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
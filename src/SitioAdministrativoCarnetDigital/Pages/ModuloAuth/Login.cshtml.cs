using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioAuth;
using System.Text.Json;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloAuth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthApiClient _authApi;
        private readonly IConfiguration _config;

        private const int MaxIntentosFallidos = 3;
        private const string SessionKeyToken = "JwtToken";
        private const string SessionKeyEmail = "UserEmail";
        private const string SessionKeyRefresh = "RefreshToken";
        private const string SessionKeyIntentos = "FailedAttempts";

        [BindProperty] public string Usuario { get; set; } = string.Empty;
        [BindProperty] public string Contrasena { get; set; } = string.Empty;

        // Mensaje visible en el formulario (error, bloqueo, sesión vencida, etc.)
        public string? AlertMessage { get; private set; }

        // JSON de la respuesta completa del /login para que el script lo guarde
        // en localStorage. Solo se rellena cuando el login fue exitoso.
        public string? LoginDataJson { get; private set; }

        public LoginModel(IAuthApiClient authApi, IConfiguration config)
        {
            _authApi = authApi;
            _config = config;
        }

        // GET: muestra el formulario.
        // El middleware de sesión redirige aquí con ?sinSesion=1 cuando el usuario
        // intenta acceder a una página protegida sin haber iniciado sesión.
        public IActionResult OnGet()
        {
            // Si ya hay sesión activa no necesita volver a loguearse.
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyToken)))
                return RedirectToPage("/Index");

            // El middleware añade sinSesion=1 cuando viene de una ruta protegida.
            if (Request.Query.ContainsKey("sinSesion"))
                AlertMessage = "Por favor inicie sesión para utilizar el sistema.";

            return Page();
        }

        // POST: procesa las credenciales enviadas desde el formulario.
        public async Task<IActionResult> OnPostAsync()
        {
            // Campos vacíos: error inmediato sin llamar al microservicio.
            if (string.IsNullOrWhiteSpace(Usuario) || string.IsNullOrWhiteSpace(Contrasena))
            {
                AlertMessage = "Usuario y/o contraseña incorrectos.";
                return Page();
            }

            // Si el contador de intentos ya alcanzó el máximo, bloquear sin llamar al microservicio.
            var intentos = HttpContext.Session.GetInt32(SessionKeyIntentos) ?? 0;
            if (intentos >= MaxIntentosFallidos)
            {
                AlertMessage = "Por motivos de seguridad, el usuario ha sido bloqueado. Contacte al administrador.";
                return Page();
            }

            // El GUID del tipo "Administrador" viene de appsettings["TipoUsuario"].
            var tipoUsuario = _config["TipoUsuario"]
                ?? throw new InvalidOperationException(
                       "'TipoUsuario' no está configurado en appsettings.");

            var result = await _authApi.LoginAsync(Usuario.Trim(), Contrasena, tipoUsuario);

            if (result.Success && result.Data is not null)
            {
                // Login correcto: reiniciar contador y guardar tokens en Session.
                HttpContext.Session.Remove(SessionKeyIntentos);
                HttpContext.Session.SetString(SessionKeyToken, result.Data.AccessToken);
                HttpContext.Session.SetString(SessionKeyEmail, result.Data.UsuarioID);   // campo real: usuarioID
                HttpContext.Session.SetString(SessionKeyRefresh, result.Data.RefreshToken);

                // Serializar la respuesta completa para que Login.js la guarde en localStorage.
                // El script cliente ejecuta storeLoginData() y luego redirige al Index.
                // No usamos RedirectToPage() aquí porque localStorage no es accesible desde servidor.
                LoginDataJson = JsonSerializer.Serialize(result.Data);
                return Page();
            }

            // Login fallido: incrementar contador de intentos fallidos en Session.
            intentos++;
            HttpContext.Session.SetInt32(SessionKeyIntentos, intentos);

            // Previsto: cuando MicroServicioAuth implemente bloqueo en BD devolverá 403.
            if (result.StatusCode == 403
                || (result.ErrorMessage?.Contains("bloqueado", StringComparison.OrdinalIgnoreCase) == true))
            {
                HttpContext.Session.SetInt32(SessionKeyIntentos, MaxIntentosFallidos);
                AlertMessage = "Por motivos de seguridad, el usuario ha sido bloqueado. Contacte al administrador.";
                return Page();
            }

            if (intentos >= MaxIntentosFallidos)
            {
                AlertMessage = "Por motivos de seguridad, el usuario ha sido bloqueado. Contacte al administrador.";
                return Page();
            }

            AlertMessage = "Usuario y/o contraseña incorrectos.";
            return Page();
        }
    }
}



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioAutoregistro;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloAutoregistro
{
    public class AutoregistroModel : PageModel
    {
        private readonly IAutoregistroApiClient _autoregistroApi;

        // GUIDs internos — nunca expuestos en la UI
        private static readonly Guid InstitucionCUC = Guid.Parse("795271E3-0063-F111-947E-E5709BBC83CF");
        private static readonly Guid RolEstudiante = Guid.Parse("98F95770-C621-43D5-A19C-F823DA457660");
        private static readonly Guid RolFuncionario = Guid.Parse("67E33ED5-748B-4742-8948-DE86FA461A90");
        private static readonly Guid TipoUsuarioEstudiante = Guid.Parse("0B4D7DA0-1438-436C-B2D4-2D3A8F3005E7");

        // Campos del formulario
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string TipoIdentID { get; set; } = string.Empty;
        [BindProperty] public string Identificacion { get; set; } = string.Empty;
        [BindProperty] public string NombreCompleto { get; set; } = string.Empty;
        [BindProperty] public string Contrasena { get; set; } = string.Empty;
        [BindProperty] public string ConfContrasena { get; set; } = string.Empty;
        [BindProperty] public string TipoUsuarioID { get; set; } = string.Empty;
        [BindProperty] public List<string> CarrerasIDs { get; set; } = new();
        [BindProperty] public List<string> AreasTrabajoIDs { get; set; } = new();
        [BindProperty] public string Telefonos { get; set; } = string.Empty;

        // Estado de la vista
        public string? MensajeExito { get; private set; }
        public string? MensajeError { get; private set; }
        public bool RegistroExitoso { get; private set; }

        public AutoregistroModel(IAutoregistroApiClient autoregistroApi)
        {
            _autoregistroApi = autoregistroApi;
        }

        // GET normal — muestra el formulario vacío
        public void OnGet() { }

        // GET /ModuloAutoregistro/Autoregistro?handler=Confirmar&token=xxx
        // Invocado desde el enlace del correo de confirmación
        public async Task OnGetConfirmarAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                MensajeError = "El enlace de confirmación no es válido.";
                return;
            }

            var result = await _autoregistroApi.ConfirmarAsync(token);

            if (result.Success)
            {
                MensajeExito = result.Data?.Message ?? "Cuenta confirmada. Ya podés iniciar sesión.";
                RegistroExitoso = true;
            }
            else
            {
                MensajeError = result.ErrorMessage ?? "No se pudo confirmar la cuenta.";
            }
        }

        // POST — envía el formulario al microservicio
        public async Task<IActionResult> OnPostAsync()
        {
            // Validaciones del lado web antes de llamar al microservicio
            if (string.IsNullOrWhiteSpace(Email))
            { MensajeError = "El email es obligatorio."; return Page(); }

            if (string.IsNullOrWhiteSpace(NombreCompleto))
            { MensajeError = "El nombre completo es obligatorio."; return Page(); }

            if (string.IsNullOrWhiteSpace(Identificacion))
            { MensajeError = "La identificación es obligatoria."; return Page(); }

            if (string.IsNullOrWhiteSpace(TipoIdentID) || !Guid.TryParse(TipoIdentID, out var tipoIdentGuid))
            { MensajeError = "Debe seleccionar un tipo de identificación."; return Page(); }

            if (string.IsNullOrWhiteSpace(TipoUsuarioID) || !Guid.TryParse(TipoUsuarioID, out var tipoUsuarioGuid))
            { MensajeError = "Debe seleccionar un tipo de usuario."; return Page(); }

            if (string.IsNullOrWhiteSpace(Contrasena))
            { MensajeError = "La contraseña es obligatoria."; return Page(); }

            if (Contrasena != ConfContrasena)
            { MensajeError = "Las contraseñas no coinciden."; return Page(); }

            // Validar dominio del correo según tipo de usuario
            bool esEstudiante = tipoUsuarioGuid == TipoUsuarioEstudiante;
            string dominioEsperado = esEstudiante ? "cuc.cr" : "cuc.ac.cr";
            string emailLower = Email.Trim().ToLower();
            string dominioEmail = emailLower.Contains('@') ? emailLower.Split('@')[1] : string.Empty;

            if (dominioEmail != dominioEsperado)
            {
                MensajeError = esEstudiante
                    ? "El correo de un Estudiante debe ser del dominio @cuc.cr."
                    : "El correo de un Funcionario debe ser del dominio @cuc.ac.cr.";
                return Page();
            }

            // Validar selección mínima según tipo
            if (esEstudiante && CarrerasIDs.Count == 0)
            { MensajeError = "Debe seleccionar al menos una carrera."; return Page(); }

            if (!esEstudiante && AreasTrabajoIDs.Count == 0)
            { MensajeError = "Debe seleccionar al menos un área de trabajo."; return Page(); }

            // Determinar rol (interno, nunca visible al usuario)
            var rolGuid = esEstudiante ? RolEstudiante : RolFuncionario;

            // Parsear GUIDs de carreras y áreas
            var carrerasGuids = CarrerasIDs
                .Where(id => Guid.TryParse(id, out _))
                .Select(id => Guid.Parse(id))
                .ToList();

            var areasGuids = AreasTrabajoIDs
                .Where(id => Guid.TryParse(id, out _))
                .Select(id => Guid.Parse(id))
                .ToList();

            // Parsear teléfonos separados por coma
            var telefonosList = string.IsNullOrWhiteSpace(Telefonos)
                ? new List<string>()
                : Telefonos.Split(',')
                           .Select(t => t.Trim())
                           .Where(t => !string.IsNullOrEmpty(t))
                           .ToList();

            // Fecha de vencimiento: un año desde hoy (automático, no visible al usuario)
            var request = new UsuarioRegistroRequest
            {
                TipoIdentID = tipoIdentGuid,
                Identificacion = Identificacion.Trim(),
                NombreCompleto = NombreCompleto.Trim(),
                Email = emailLower,
                Contrasena = Contrasena,
                InstitucionID = InstitucionCUC,
                TipoUsuarioID = tipoUsuarioGuid,
                RolID = rolGuid,
                FechaVencimientoCarnet = DateTime.UtcNow.AddYears(1),
                CarrerasIDs = esEstudiante ? carrerasGuids : new(),
                AreasTrabajoIDs = esEstudiante ? new() : areasGuids,
                Telefonos = telefonosList
            };

            var result = await _autoregistroApi.RegistrarAsync(request);

            if (result.Success)
            {
                MensajeExito = result.Data?.Message ?? "Registro exitoso. Revisá tu email para confirmar tu cuenta.";
                RegistroExitoso = true;
            }
            else
            {
                MensajeError = result.ErrorMessage ?? "No se pudo completar el registro.";
            }

            return Page();
        }
    }
}



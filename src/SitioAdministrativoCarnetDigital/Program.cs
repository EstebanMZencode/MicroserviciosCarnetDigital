var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// ============================================
// SESIÓN — almacena el JWT Token del usuario
// ============================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
});

// ============================================
// INYECCIÓN DE DEPENDENCIAS - MICROSERVICIOS
// ============================================

// HttpClient para consumir cada microservicio REST
// Las URLs base se configuran en appsettings.json

// MicroServicioAuth — Login
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioAuth.IAuthApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioAuth.AuthApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:AuthUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:AuthUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioAreasTrabajo
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioAreasTrabajo.IAreasTrabajoApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioAreasTrabajo.AreasTrabajoApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:AreasTrabajoUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:AreasTrabajoUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioAutoregistro
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioAutoregistro.IAutoregistroApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioAutoregistro.AutoregistroApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:AutoregistroUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:AutoregistroUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioBitacoras
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioBitacoras.IBitacorasApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioBitacoras.BitacorasApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:BitacoraUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:BitacoraUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioCarreras
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioCarreras.ICarrerasApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioCarreras.CarrerasApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:CarrerasUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:CarrerasUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioEstadosUsuario
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioEstadosUsuario.IEstadosUsuarioApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioEstadosUsuario.EstadosUsuarioApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:EstadosUsuarioUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:EstadosUsuarioUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioFotografias
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioFotografias.IFotografiasApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioFotografias.FotografiasApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:FotografiasUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:FotografiasUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioInstituciones
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioInstituciones.IInstitucionesApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioInstituciones.InstitucionesApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:InstitucionesUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:InstitucionesUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioPantallas
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioPantallas.IPantallasApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioPantallas.PantallasApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:PantallasUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:PantallasUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioParametros
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioParametros.IParametrosApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioParametros.ParametrosApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:ParametrosUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:ParametrosUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioQRs
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioQRs.IQRsApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioQRs.QRsApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:QRsUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:QRsUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioRoles
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioRoles.IRolesApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioRoles.RolesApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:RolesUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:RolesUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioTiposIdentificacion
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioTiposIdentificacion.ITiposIdentificacionApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioTiposIdentificacion.TiposIdentificacionApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:TipoIdentificacionUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:TipoIdentificacionUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioTiposUsuario
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioTiposUsuario.ITiposUsuarioApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioTiposUsuario.TiposUsuarioApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:TiposUsuarioUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:TiposUsuarioUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

// MicroServicioUsuarios
builder.Services.AddHttpClient<
    SitioAdministrativoCarnetDigital.Services.MicroServicioUsuarios.IUsuariosApiClient,
    SitioAdministrativoCarnetDigital.Services.MicroServicioUsuarios.UsuariosApiClient>(client =>
    {
        var baseUrl = builder.Configuration["MicroServicios:UsuariosUrl"]
                      ?? throw new InvalidOperationException("MicroServicios:UsuariosUrl no configurado");
        client.BaseAddress = new Uri(baseUrl);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Archivos estáticos de la carpeta Pages/ (CSS y JS por módulo).
// Pages/ModuloRoles/Roles.css → ~/pages-static/ModuloRoles/Roles.css
// Pages/ModuloAuth/Login.css  → ~/pages-static/ModuloAuth/Login.css
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Pages")),
    RequestPath = "/pages-static"
});

app.UseHttpsRedirection();

// Archivos estáticos de wwwroot (Bootstrap, jQuery, lib/, etc.)
app.UseStaticFiles();

app.UseRouting();

// La sesión debe estar activa antes del middleware que la consulta.
app.UseSession();

// ── Middleware global de autorización por sesión ──────────────────────────────
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;

    var esPublico =
        path.StartsWith("/ModuloAuth/Login", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/ModuloAutoregistro/Autoregistro", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/pages-static/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/_", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/Error", StringComparison.OrdinalIgnoreCase);

    if (!esPublico)
    {
        var token = context.Session.GetString("JwtToken");

        if (string.IsNullOrEmpty(token))
        {
            // sinSesion=1 le indica al Login que muestre el mensaje
            // "Por favor inicie sesión para utilizar el sistema."
            // Solo se agrega cuando el usuario venía de una página real.
            var destino = path.Equals("/", StringComparison.OrdinalIgnoreCase)
                ? "/ModuloAuth/Login"
                : "/ModuloAuth/Login?sinSesion=1";

            context.Response.Redirect(destino);
            return;
        }
    }

    await next(context);
});

app.UseAuthorization();

app.MapRazorPages();

app.Run();

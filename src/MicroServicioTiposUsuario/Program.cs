using MicroservicioTiposUsuario;
using MicroservicioTiposUsuario.Repository;
using MicroservicioTiposUsuario.Services;
using MicroServicioTiposUsuario.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=tiusr23pl.cuc-carrera-ti.ac.cr;Database=tiusr23pl_Carnet_Identity;User Id=Carnet_Identity_User;Password=Carnet_Identity123;Encrypt=false;TrustServerCertificate=true;";

builder.Services.AddDbContext<TipoUsuarioDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ITipoUsuarioRepository, TipoUsuarioRepository>();
builder.Services.AddScoped<ITipoUsuarioService, TipoUsuarioService>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TipoUsuario API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger") ||
        context.Request.Path.StartsWithSegments("/openapi"))
    {
        await next();
        return;
    }

    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

    if (string.IsNullOrEmpty(token))
    {
        token = context.Request.Headers["token"].ToString();
    }

    if (string.IsNullOrEmpty(token))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error = "Token requerido" });
        return;
    }

    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    using var httpClient = new HttpClient(handler);
    httpClient.Timeout = TimeSpan.FromSeconds(10);

    try
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            "https://tiusr23pl.cuc-carrera-ti.ac.cr/MicroServicioAuth/api/validate");
        request.Headers.Add("token", token);

        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Token inválido o expirado" });
            return;
        }
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Error validando token: " + ex.Message });
        return;
    }

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TipoUsuario API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapTipoUsuarioEndpoints();

app.Run();
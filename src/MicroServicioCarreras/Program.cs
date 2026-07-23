using MicroservicioCarreras;
using MicroservicioCarreras.Repository;
using MicroservicioCarreras.Services;
using MicroServicioCarreras.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=tiusr23pl.cuc-carrera-ti.ac.cr;Database=tiusr23pl_Carnet_Core;User Id=Carnet_Core_User;Password=Carnet_Core123;Encrypt=false;TrustServerCertificate=true;";

builder.Services.AddDbContext<CarreraDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<ICarreraService, CarreraService>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Carrera API", Version = "v1" });
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

    using var httpClient = new HttpClient();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Carrera API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapCarreraEndpoints();  

app.Run();
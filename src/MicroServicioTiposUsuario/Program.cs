using MicroservicioTiposUsuario;
using MicroservicioTiposUsuario.Entities;
using MicroservicioTiposUsuario.Repository;
using MicroservicioTiposUsuario.Services;
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
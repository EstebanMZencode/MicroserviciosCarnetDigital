using FluentValidation;
using MicroServicioUsuarios;
using MicroServicioUsuarios.Entities;
using MicroServicioUsuarios.Repository;
using MicroServicioUsuarios.Services;
using MicroServicioUsuarios.Services.ExternalServices;
using MicroServicioUsuarios.Services.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{ options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// http client para MicroServicios
builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inyección de Dependencias
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Validadores inyectados
builder.Services.AddScoped<IValidator<UsuarioRequest>, UsuarioRequestValidator>();
builder.Services.AddScoped<IValidator<PerfilUsuarioRequest>, PerfilUsuarioRequestValidator>();
builder.Services.AddScoped<IValidator<LoginUsuarioRequest>, LoginUsuarioRequestValidator>();

// Microservicios externos
builder.Services.AddScoped<IAuthE_Service, AuthE_Service>();
builder.Services.AddScoped<ITipoIdentificacionE_Service, TipoIdentificacionE_Service>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MicroServicioUsuarios v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapUsuariosEndpoints();
app.Run();

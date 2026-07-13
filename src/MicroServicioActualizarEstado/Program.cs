using MicroServicioActualizarEstadoUsuario;
using MicroServicioActualizarEstadoUsuario.Repository;
using MicroServicioActualizarEstadoUsuario.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// HTTP Client para MicroServicioAuth (/validate) y MicroServicioBitacoras
builder.Services.AddHttpClient();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inyección de dependencias (con interfaces, como tu microservicio de Roles)
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IEstadoUsuarioRepository, EstadoUsuarioRepository>();
builder.Services.AddScoped<IEstadoUsuarioService, EstadoUsuarioService>();

var app = builder.Build();

// Swagger en todos los entornos (como tenías)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MicroServicioEstadoUsuario v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapEstadoUsuarioEndpoints();
app.Run();
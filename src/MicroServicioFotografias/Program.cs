using MicroServicioFotografias;
using MicroServicioFotografias.Repository;
using MicroServicioFotografias.Services;

var builder = WebApplication.CreateBuilder(args);

// La fotografía viaja en un header HTTP. Kestrel tiene un límite por defecto de 32 KB
// para el total de headers. Se amplía a 4 MB para soportar imágenes Base64 de hasta 1 MB
// (Base64 incrementa el tamaño ~33% respecto a los bytes originales).
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBufferSize = 4 * 1024 * 1024;
    options.Limits.MaxRequestHeadersTotalSize = 4 * 1024 * 1024; // 4 MB
    options.Limits.MaxRequestHeaderCount = 100;
});

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

// Inyeccion de dependencias
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<FotografiasRepository>();
builder.Services.AddScoped<IFotografiasService, FotografiasService>();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.MapFotografiasEndpoints();

app.Run();

using MicroservicioCarreras;
using MicroservicioCarreras.Entities;
using MicroservicioCarreras.Repository;
using MicroservicioCarreras.Services;
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
using MicroservicioAreas;
using MicroservicioAreas.Entities;
using MicroservicioAreas.Repository;
using MicroservicioAreas.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=tiusr23pl.cuc-carrera-ti.ac.cr;Database=tiusr23pl_Carnet_Core;User Id=Carnet_Core_User;Password=Carnet_Core123;Encrypt=false;TrustServerCertificate=true;";

builder.Services.AddDbContext<AreaDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<IAreaService, AreaService>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Area API", Version = "v1" });
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Area API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapAreaEndpoints();

app.Run();
using MicroServicioRoles;
using MicroServicioRoles.Repository;
using MicroServicioRoles.Services;

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

// Inyeccion de dependencias
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<RolesRepository>();
builder.Services.AddScoped<IRolesService, RolesService>();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.MapRolesEndpoints();

app.Run();


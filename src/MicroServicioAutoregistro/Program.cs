using MicroServicioAutoregistro;
using MicroServicioAutoregistro.Repository;
using MicroServicioAutoregistro.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<AutoregistroRepository>();
builder.Services.AddScoped<IAutoregistroService, AutoregistroService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapAutoregistroEndpoints();

app.Run();
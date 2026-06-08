using MicroServicioPantallas;
using MicroServicioPantallas.Repository;
using MicroServicioPantallas.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<PantallaRepository>();
builder.Services.AddScoped<IPantallaService, PantallaService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("FrontDev");
app.MapPantallaEndpoints();
app.Run();
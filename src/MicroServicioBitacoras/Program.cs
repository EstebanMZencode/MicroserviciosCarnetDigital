using MicroServicioBitacoras;
using MicroServicioBitacoras.Repository;
using MicroServicioBitacoras.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS para consumo desde el front (ajustar orígenes según el equipo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontDev", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Inyección de dependencias
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<BitacoraRepository>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();

var app = builder.Build();

// Pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("FrontDev");

app.MapBitacoraEndpoints();
app.Run();
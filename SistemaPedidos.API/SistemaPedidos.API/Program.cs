using Microsoft.EntityFrameworkCore;
using SistemaPedidos.Application.Interfaces;
using SistemaPedidos.Application.Services;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Constants;
using SistemaPedidos.Infrastructure.Data;
using SistemaPedidos.Infrastructure.Repositories;
using SistemaPedidos.Infrastructure.Services;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Sistema Pedidos API", 
        Version = "v1",
        Description = "API para gestión de pedidos con validación externa y auditoría"
    });
});

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<SistemaPedidosDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        )
    )
);

// Leer configuración para HttpClient usando las constantes
var baseUrl = builder.Configuration[ConfigurationKeys.VALIDACION_BASE_URL] 
    ?? throw new InvalidOperationException("La configuración 'ValidacionExterna:BaseUrl' es requerida");

var timeoutSeconds = int.TryParse(
    builder.Configuration[ConfigurationKeys.VALIDACION_TIMEOUT], 
    out var timeout
) ? timeout : 10;

// Configurar HttpClient para el servicio externo
builder.Services.AddHttpClient("JSONPlaceholder", client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json")
    );
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});

// Dependencias - Infrastructure
builder.Services.AddScoped<IOrkestador, Orkestador>();
builder.Services.AddScoped<IValidacionExternaService, ValidacionExternaService>();

// Dependencias - Application
builder.Services.AddScoped<IPedidoService, PedidoService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Pedidos API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

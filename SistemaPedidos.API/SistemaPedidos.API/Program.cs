using Microsoft.EntityFrameworkCore;
using SistemaPedidos.Application.Interfaces;
using SistemaPedidos.Application.Services;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Data;
using SistemaPedidos.Infrastructure.Repositories;
using SistemaPedidos.Infrastructure.Services;
using SistemaPedidos.API.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Iniciando aplicación SistemaPedidos");

    // Add services to the container
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

    // DbContext con estrategia de reintentos
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' no encontrado");

    builder.Services.AddDbContext<SistemaPedidosDbContext>(options =>
        options.UseSqlServer(
            connectionString,
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            )
        )
    );

    // HttpClient para servicio externo
    var baseUrl = builder.Configuration["ValidacionExterna:BaseUrl"]
        ?? throw new InvalidOperationException("ValidacionExterna:BaseUrl no configurado");
    var timeoutSeconds = builder.Configuration.GetValue<int>("ValidacionExterna:TimeoutSeconds");

    builder.Services.AddHttpClient("JSONPlaceholder", client =>
    {
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    });

    // Dependencias - Infrastructure
    builder.Services.AddScoped<IOrkestador, Orkestador>();
    builder.Services.AddScoped<IValidacionExternaService, ValidacionExternaService>();

    // Dependencias - Application
    builder.Services.AddScoped<IPedidoService, PedidoService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseGlobalExceptionHandler();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Aplicación configurada exitosamente");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

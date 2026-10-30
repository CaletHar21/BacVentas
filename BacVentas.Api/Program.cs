using BacVentas.Api.Data;
using BacVentas.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Servicios (inyección de dependencias) ----------

// Entity Framework con SQL Server. La cadena de conexión está en appsettings.json.
builder.Services.AddDbContext<BacVentasContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BacVentas")));

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Cuando un DTO no pasa la validación, respondemos con el mismo formato
        // que el resto de errores: { "mensaje": "..." }, en vez del formato por defecto.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errores = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return new BadRequestObjectResult(new { mensaje = string.Join(" ", errores) });
        };
    });

// Swagger: página para documentar y probar la API desde el navegador.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---------- Datos iniciales ----------
// Se crea un "scope" porque el DbContext vive por petición y aquí no hay petición.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<BacVentasContext>();
        await DbSeeder.CrearUsuariosAsync(db, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudieron crear los usuarios de prueba. ¿Ejecutaste el script SQL?");
    }
}

// ---------- Pipeline (orden en que pasa cada petición) ----------

// Va primero para atrapar los errores de todo lo que sigue.
app.UseMiddleware<ErrorHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

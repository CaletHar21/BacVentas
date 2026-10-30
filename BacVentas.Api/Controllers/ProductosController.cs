using BacVentas.Api.Data;
using BacVentas.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BacVentas.Api.Controllers;

// [ApiController]: valida automáticamente los DTOs y responde 400 si son inválidos.
// [Route("api/[controller]")]: la ruta será /api/productos ([controller] = nombre sin "Controller").
[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly BacVentasContext _db;

    // Inyección de dependencias: ASP.NET crea el DbContext y lo pasa aquí.
    public ProductosController(BacVentasContext db) => _db = db;

    // GET /api/productos
    [HttpGet]
    public async Task<ActionResult<List<Producto>>> Listar()
    {
        return await _db.Productos
            .FromSqlRaw("EXEC dbo.SP_PRODUCTOS_LISTAR")
            .AsNoTracking()   // solo lectura: EF no necesita "seguir" los cambios
            .ToListAsync();
    }

    // GET /api/productos/1  -> buscar por código
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> Obtener(int id)
    {
        var producto = (await _db.Productos
            .FromSqlInterpolated($"EXEC dbo.SP_PRODUCTOS_OBTENERPORID @IDPRO = {id}")
            .AsNoTracking()
            .ToListAsync()).FirstOrDefault();

        return producto is null
            ? NotFound(new { mensaje = $"No existe un producto con el código {id:D3}." })
            : Ok(producto);
    }

    // POST /api/productos
    [HttpPost]
    public async Task<ActionResult<Producto>> Crear([FromBody] ProductoRequest req)
    {
        var nombre = req.Nombre.Trim().ToUpper();   // se guarda en MAYÚSCULAS

        var creado = (await _db.Productos
            .FromSqlInterpolated($"EXEC dbo.SP_PRODUCTOS_INSERTAR @PRODUCTO = {nombre}, @PRECIO = {req.Precio}")
            .AsNoTracking()
            .ToListAsync()).First();

        // 201 Created + cabecera Location con la URL del nuevo producto (buena práctica REST).
        return CreatedAtAction(nameof(Obtener), new { id = creado.IdPro }, creado);
    }

    // PUT /api/productos/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Producto>> Actualizar(int id, [FromBody] ProductoRequest req)
    {
        var nombre = req.Nombre.Trim().ToUpper();

        // Si no existe, el SP lanza THROW 50001 y el middleware responde 404.
        var actualizado = (await _db.Productos
            .FromSqlInterpolated($"EXEC dbo.SP_PRODUCTOS_ACTUALIZAR @IDPRO = {id}, @PRODUCTO = {nombre}, @PRECIO = {req.Precio}")
            .AsNoTracking()
            .ToListAsync()).First();

        return Ok(actualizado);
    }

    // DELETE /api/productos/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        // Si tiene ventas, el SP lanza THROW 50002 y el middleware responde 400 con el mensaje.
        await _db.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.SP_PRODUCTOS_ELIMINAR @IDPRO = {id}");
        return Ok(new { mensaje = "Producto eliminado correctamente." });
    }
}

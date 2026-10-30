using System.Text.Json;
using BacVentas.Api.Data;
using BacVentas.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BacVentas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly BacVentasContext _db;

    public VentasController(BacVentasContext db) => _db = db;

    // POST /api/ventas  -> registra encabezado + detalle
    [HttpPost]
    public async Task<ActionResult<Venta>> Registrar([FromBody] VentaRequest req)
    {
        // Si el mismo producto viene dos veces, se suman las cantidades.
        // Las propiedades en MAYÚSCULAS coinciden con las rutas '$.IDPRO' y '$.CANTIDAD'
        // que lee OPENJSON dentro del SP.
        var detalle = req.Detalle
            .GroupBy(d => d.IdPro)
            .Select(g => new { IDPRO = g.Key, CANTIDAD = g.Sum(x => x.Cantidad) });

        var json = JsonSerializer.Serialize(detalle);

        var venta = (await _db.Ventas
            .FromSqlInterpolated($"EXEC dbo.SP_VENTAS_REGISTRAR @VENDEDOR = {req.Vendedor}, @DETALLE = {json}")
            .AsNoTracking()
            .ToListAsync()).First();

        return Ok(venta);
    }

    // GET /api/ventas?desde=2026-10-01&hasta=2026-10-31  (fechas opcionales)
    [HttpGet]
    public async Task<ActionResult<List<VentaResumen>>> Listar([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return await _db.VentasResumen
            .FromSqlInterpolated($"EXEC dbo.SP_VENTAS_LISTAR @DESDE = {desde?.Date}, @HASTA = {hasta?.Date}")
            .ToListAsync();
    }

    // GET /api/ventas/1/detalle
    [HttpGet("{id:int}/detalle")]
    public async Task<ActionResult<List<VentaDetalleItem>>> Detalle(int id)
    {
        return await _db.VentasDetalle
            .FromSqlInterpolated($"EXEC dbo.SP_VENTAS_DETALLE @IDVENTA = {id}")
            .ToListAsync();
    }
}

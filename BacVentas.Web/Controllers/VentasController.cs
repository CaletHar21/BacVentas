using BacVentas.Web.Models;
using BacVentas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BacVentas.Web.Controllers;

// Ambos roles pueden registrar ventas.
[Authorize(Roles = "ADMIN,OPERADOR")]
public class VentasController : Controller
{
    private readonly ApiClient _api;

    public VentasController(ApiClient api) => _api = api;

    public IActionResult Index() => View();

    // GET /Ventas/BuscarProducto/1 -> lo usa ventas.js al escribir el código.
    // El navegador llama a la Web (no directo a la API): así la API no queda expuesta
    // y no hay que configurar CORS.
    [HttpGet]
    public async Task<IActionResult> BuscarProducto(int id)
    {
        try
        {
            var producto = await _api.ObtenerProductoAsync(id);
            return producto is null
                ? NotFound(new { mensaje = $"No existe un producto con el código {id:D3}." })
                : Json(producto);
        }
        catch (ApiException ex)
        {
            return StatusCode(503, new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar([FromBody] VentaWebRequest req)
    {
        if (req.Detalle is null || req.Detalle.Count == 0)
            return BadRequest(new { mensaje = "Agregue al menos un producto a la venta." });

        // El vendedor es el usuario que inició sesión (claim), no un dato del navegador.
        var vendedor = User.FindFirst("NombreCompleto")?.Value ?? User.Identity!.Name!;

        try
        {
            var venta = await _api.RegistrarVentaAsync(vendedor, req.Detalle);
            return Json(venta);
        }
        catch (ApiException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}

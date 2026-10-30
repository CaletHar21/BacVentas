using BacVentas.Web.Models;
using BacVentas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BacVentas.Web.Controllers;

// Solo el administrador puede gestionar productos.
// Un OPERADOR que intente entrar es enviado a /Cuenta/AccesoDenegado.
[Authorize(Roles = "ADMIN")]
public class ProductosController : Controller
{
    private readonly ApiClient _api;

    public ProductosController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        try
        {
            return View(await _api.ListarProductosAsync());
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View(new List<ProductoVm>());
        }
    }

    [HttpGet]
    public IActionResult Crear() => View(new ProductoVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProductoVm model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var creado = await _api.CrearProductoAsync(model);
            // TempData sobrevive a la redirección; el layout lo muestra con SweetAlert.
            TempData["Exito"] = $"Producto {creado.IdPro:D3} creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var producto = await _api.ObtenerProductoAsync(id);
            if (producto is null)
            {
                TempData["Error"] = "El producto no existe.";
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ProductoVm model)
    {
        model.IdPro = id;
        if (!ModelState.IsValid) return View(model);
        try
        {
            await _api.ActualizarProductoAsync(model);
            TempData["Exito"] = $"Producto {id:D3} actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // Se llama con fetch() desde la lista (después de confirmar con SweetAlert).
    // Responde JSON para que el JavaScript muestre el resultado sin recargar la página.
    [HttpPost]
    [ValidateAntiForgeryToken]   // el token viaja en la cabecera "RequestVerificationToken"
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            await _api.EliminarProductoAsync(id);
            return Json(new { ok = true, mensaje = "Producto eliminado correctamente." });
        }
        catch (ApiException ex)
        {
            // Ej.: "No se puede eliminar: el producto tiene ventas registradas."
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }
}

using System.Net;
using System.Net.Http.Json;
using BacVentas.Web.Models;

namespace BacVentas.Web.Services;

// Error devuelto por la API, con el mensaje listo para mostrar al usuario.
public class ApiException : Exception
{
    public int StatusCode { get; }
    public ApiException(string mensaje, int statusCode = 0) : base(mensaje) => StatusCode = statusCode;
}

// Centraliza TODAS las llamadas HTTP a la API.
// Los controladores no conocen URLs ni JSON: solo llaman métodos como ListarProductosAsync().
// GetFromJsonAsync / PostAsJsonAsync usan JSON en camelCase, igual que la API.
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    // ---------- Autenticación ----------
    public async Task<UsuarioSesion?> LoginAsync(string usuario, string password)
    {
        var resp = await EnviarAsync(() => _http.PostAsJsonAsync("api/auth/login", new { usuario, password }));
        if (resp.StatusCode == HttpStatusCode.Unauthorized) return null;   // credenciales incorrectas
        return await LeerAsync<UsuarioSesion>(resp);
    }

    // ---------- Productos ----------
    public async Task<List<ProductoVm>> ListarProductosAsync()
        => await LeerAsync<List<ProductoVm>>(await EnviarAsync(() => _http.GetAsync("api/productos")));

    public async Task<ProductoVm?> ObtenerProductoAsync(int id)
    {
        var resp = await EnviarAsync(() => _http.GetAsync($"api/productos/{id}"));
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        return await LeerAsync<ProductoVm>(resp);
    }

    public async Task<ProductoVm> CrearProductoAsync(ProductoVm p)
        => await LeerAsync<ProductoVm>(await EnviarAsync(() =>
            _http.PostAsJsonAsync("api/productos", new { nombre = p.Nombre, precio = p.Precio })));

    public async Task<ProductoVm> ActualizarProductoAsync(ProductoVm p)
        => await LeerAsync<ProductoVm>(await EnviarAsync(() =>
            _http.PutAsJsonAsync($"api/productos/{p.IdPro}", new { nombre = p.Nombre, precio = p.Precio })));

    public async Task EliminarProductoAsync(int id)
    {
        var resp = await EnviarAsync(() => _http.DeleteAsync($"api/productos/{id}"));
        await ValidarAsync(resp);
    }

    // ---------- Ventas ----------
    public async Task<VentaCreadaVm> RegistrarVentaAsync(string vendedor, List<VentaItemVm> detalle)
        => await LeerAsync<VentaCreadaVm>(await EnviarAsync(() =>
            _http.PostAsJsonAsync("api/ventas", new { vendedor, detalle })));

    public async Task<List<VentaResumenVm>> ListarVentasAsync(DateTime? desde, DateTime? hasta)
    {
        // Fechas en formato ISO (yyyy-MM-dd) para que la API las interprete sin ambigüedad.
        var filtros = new List<string>();
        if (desde.HasValue) filtros.Add($"desde={desde:yyyy-MM-dd}");
        if (hasta.HasValue) filtros.Add($"hasta={hasta:yyyy-MM-dd}");
        var url = "api/ventas" + (filtros.Count > 0 ? "?" + string.Join("&", filtros) : "");

        return await LeerAsync<List<VentaResumenVm>>(await EnviarAsync(() => _http.GetAsync(url)));
    }

    public async Task<List<VentaDetalleVm>> DetalleVentaAsync(int idVenta)
        => await LeerAsync<List<VentaDetalleVm>>(await EnviarAsync(() => _http.GetAsync($"api/ventas/{idVenta}/detalle")));

    // ---------- Helpers ----------

    // Si la API está apagada, HttpClient lanza HttpRequestException; la convertimos en un mensaje claro.
    private static async Task<HttpResponseMessage> EnviarAsync(Func<Task<HttpResponseMessage>> llamada)
    {
        try
        {
            return await llamada();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException("No se pudo conectar con la API. Verifique que BacVentas.Api esté en ejecución.");
        }
    }

    private static async Task<T> LeerAsync<T>(HttpResponseMessage resp)
    {
        await ValidarAsync(resp);
        return (await resp.Content.ReadFromJsonAsync<T>())!;
    }

    // La API siempre responde los errores como { "mensaje": "..." } (ver ErrorHandlerMiddleware).
    private static async Task ValidarAsync(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;

        string mensaje;
        try
        {
            var error = await resp.Content.ReadFromJsonAsync<ErrorApi>();
            mensaje = string.IsNullOrWhiteSpace(error?.Mensaje) ? $"Error {(int)resp.StatusCode} en la API." : error.Mensaje;
        }
        catch
        {
            mensaje = $"Error {(int)resp.StatusCode} en la API.";
        }
        throw new ApiException(mensaje, (int)resp.StatusCode);
    }

    private class ErrorApi
    {
        public string? Mensaje { get; set; }
    }
}

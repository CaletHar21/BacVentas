using System.ComponentModel.DataAnnotations;

namespace BacVentas.Web.Models;

// ViewModels = clases que usan las vistas y que reciben/envían JSON a la API.
// Los nombres de propiedades coinciden con el JSON de la API (idPro, nombre, precio...).

public class ProductoVm
{
    public int IdPro { get; set; }

    [Display(Name = "Producto")]
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Precio ($)")]
    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 99999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }
}

public class LoginVm
{
    [Display(Name = "Usuario")]
    [Required(ErrorMessage = "Ingrese su usuario.")]
    public string Usuario { get; set; } = string.Empty;

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "Ingrese su contraseña.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

// Respuesta de POST api/auth/login
public class UsuarioSesion
{
    public string Usuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class VentaItemVm
{
    public int IdPro { get; set; }
    public int Cantidad { get; set; }
}

// Lo que envía la pantalla de ventas (JavaScript) al controlador Web.
// El vendedor NO viene del navegador: se toma del usuario que inició sesión.
public class VentaWebRequest
{
    public List<VentaItemVm> Detalle { get; set; } = new();
}

// Respuesta de POST api/ventas
public class VentaCreadaVm
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public string Vendedor { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

// Respuesta de GET api/ventas (reportes)
public class VentaResumenVm
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public string Vendedor { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

// Respuesta de GET api/ventas/{id}/detalle
public class VentaDetalleVm
{
    public int IdDe { get; set; }
    public int IdPro { get; set; }
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

// Filtros y datos de la pantalla de reportes.
public class ReporteVentasVm
{
    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Hasta { get; set; }

    public List<VentaResumenVm> Ventas { get; set; } = new();
}

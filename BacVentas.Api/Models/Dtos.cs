using System.ComponentModel.DataAnnotations;

namespace BacVentas.Api.Models;

// DTO = Data Transfer Object: lo que el cliente ENVÍA a la API.
// Se separa de la entidad para no exponer campos internos (ej. IdPro al crear)
// y para validar la entrada con atributos ([Required], [Range]...).
// Si una validación falla, [ApiController] responde 400 automáticamente.

public class ProductoRequest
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(0.01, 99999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }
}

public class LoginRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

// Lo que la API devuelve al hacer login (sin el hash de la contraseña).
public class LoginResponse
{
    public string Usuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class VentaItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Código de producto inválido.")]
    public int IdPro { get; set; }

    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }
}

// Solo se envía IdPro y Cantidad: el precio, IVA y totales los calcula
// el SP con los precios reales de la base de datos.
public class VentaRequest
{
    [Required(ErrorMessage = "El vendedor es obligatorio.")]
    [StringLength(100)]
    public string Vendedor { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "Agregue al menos un producto a la venta.")]
    public List<VentaItemRequest> Detalle { get; set; } = new();
}

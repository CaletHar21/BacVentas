using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BacVentas.Api.Models;

// Entidad que representa la tabla PRODUCTOS.
// [Table] y [Column] conectan los nombres de C# (PascalCase, convención .NET)
// con los nombres de SQL Server (MAYÚSCULAS).
[Table("PRODUCTOS")]
public class Producto
{
    [Key]
    [Column("IDPRO")]
    public int IdPro { get; set; }

    // Se llama "Nombre" y no "Producto" porque en C# una propiedad
    // no puede llamarse igual que su clase.
    [Column("PRODUCTO")]
    public string Nombre { get; set; } = string.Empty;

    // [Precision] = DECIMAL(10,2) en SQL. Para dinero siempre decimal, nunca double.
    [Column("PRECIO")]
    [Precision(10, 2)]
    public decimal Precio { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BacVentas.Api.Models;

// Encabezado de venta (tabla VENTAS).
[Table("VENTAS")]
public class Venta
{
    [Key]
    [Column("IDVENTA")]
    public int IdVenta { get; set; }

    [Column("FECHA")]
    public DateTime Fecha { get; set; }

    [Column("VENDEDOR")]
    public string Vendedor { get; set; } = string.Empty;

    [Column("TOTAL")]
    [Precision(12, 2)]
    public decimal Total { get; set; }
}

// [Keyless] = clase sin llave primaria. No es una tabla: solo recibe
// el resultado de un procedimiento almacenado (SP_VENTAS_LISTAR).
[Keyless]
public class VentaResumen
{
    [Column("IDVENTA")] public int IdVenta { get; set; }
    [Column("FECHA")] public DateTime Fecha { get; set; }
    [Column("VENDEDOR")] public string Vendedor { get; set; } = string.Empty;
    [Column("SUBTOTAL")][Precision(12, 2)] public decimal Subtotal { get; set; }
    [Column("IVA")][Precision(12, 2)] public decimal Iva { get; set; }
    [Column("TOTAL")][Precision(12, 2)] public decimal Total { get; set; }
}

// Resultado de SP_VENTAS_DETALLE (detalle + nombre del producto).
[Keyless]
public class VentaDetalleItem
{
    [Column("IDDE")] public int IdDe { get; set; }
    [Column("FECHA")] public DateTime Fecha { get; set; }
    [Column("IDVENTA")] public int IdVenta { get; set; }
    [Column("IDPRO")] public int IdPro { get; set; }
    [Column("PRODUCTO")] public string Producto { get; set; } = string.Empty;
    [Column("CANTIDAD")] public int Cantidad { get; set; }
    [Column("PRECIO")][Precision(10, 2)] public decimal Precio { get; set; }
    [Column("IVA")][Precision(10, 2)] public decimal Iva { get; set; }
    [Column("TOTAL")][Precision(12, 2)] public decimal Total { get; set; }
}

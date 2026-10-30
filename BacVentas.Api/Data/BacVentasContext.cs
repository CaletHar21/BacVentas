using BacVentas.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BacVentas.Api.Data;

// DbContext = la "puerta" de Entity Framework hacia la base de datos.
// Cada DbSet representa un conjunto de datos que podemos consultar.
// En este proyecto NO usamos LINQ contra las tablas: todo pasa por
// procedimientos almacenados con FromSql / ExecuteSql (requisito de la prueba).
public class BacVentasContext : DbContext
{
    public BacVentasContext(DbContextOptions<BacVentasContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaResumen> VentasResumen => Set<VentaResumen>();
    public DbSet<VentaDetalleItem> VentasDetalle => Set<VentaDetalleItem>();
}

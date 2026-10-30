using Microsoft.EntityFrameworkCore;

namespace BacVentas.Api.Data;

// Crea los usuarios de prueba al iniciar la API si todavía no existen.
// Se hace aquí (y no en el script SQL) porque la contraseña debe cifrarse
// con BCrypt, y eso se hace en C#.
public static class DbSeeder
{
    public static async Task CrearUsuariosAsync(BacVentasContext db, ILogger logger)
    {
        var usuarios = new[]
        {
            (Usuario: "admin",    Nombre: "ADMINISTRADOR", Password: "Admin123*",    Rol: "ADMIN"),
            (Usuario: "usuario1", Nombre: "JUAN PEREZ",    Password: "Operador123*", Rol: "OPERADOR")
            
        };

        foreach (var u in usuarios)
        {
            
            var existe = (await db.Usuarios
                .FromSqlInterpolated($"EXEC dbo.SP_USUARIOS_OBTENERPORUSUARIO @USUARIO = {u.Usuario}")
                .AsNoTracking()
                .ToListAsync()).Any();

            if (existe) continue;

            // HashPassword genera un "salt" aleatorio: la misma contraseña
            // produce un hash distinto cada vez, y aun así Verify() funciona.
            var hash = BCrypt.Net.BCrypt.HashPassword(u.Password);

            // $"..." con FromSqlInterpolated/ExecuteSqlInterpolated NO concatena texto:
            // EF convierte cada {valor} en un parámetro SQL (@p0, @p1...).
            // Esto previene inyección SQL.
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC dbo.SP_USUARIOS_INSERTAR @USUARIO = {u.Usuario}, @NOMBRECOMPLETO = {u.Nombre}, @PASSWORDHASH = {hash}, @ROL = {u.Rol}");

            logger.LogInformation("Usuario de prueba creado: {Usuario}", u.Usuario);
        }
    }
}

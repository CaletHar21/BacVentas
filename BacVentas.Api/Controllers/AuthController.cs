using BacVentas.Api.Data;
using BacVentas.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BacVentas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BacVentasContext _db;

    public AuthController(BacVentasContext db) => _db = db;

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var usuario = (await _db.Usuarios
            .FromSqlInterpolated($"EXEC dbo.SP_USUARIOS_OBTENERPORUSUARIO @USUARIO = {req.Usuario.Trim()}")
            .AsNoTracking()
            .ToListAsync()).FirstOrDefault();

        // BCrypt.Verify compara la contraseña escrita con el hash guardado.
        // Se usa el MISMO mensaje si el usuario no existe o si la contraseña es incorrecta,
        // para no revelar qué usuarios existen (buena práctica de seguridad).
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Password, usuario.PasswordHash))
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

        return Ok(new LoginResponse
        {
            Usuario = usuario.NombreUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol
        });
    }
}

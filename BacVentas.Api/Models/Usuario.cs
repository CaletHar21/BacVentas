using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BacVentas.Api.Models;

[Table("USUARIOS")]
public class Usuario
{
    [Key]
    [Column("IDUSUARIO")]
    public int IdUsuario { get; set; }

    [Column("USUARIO")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Column("NOMBRECOMPLETO")]
    public string NombreCompleto { get; set; } = string.Empty;

    // Hash BCrypt. Nunca se envía al navegador.
    [Column("PASSWORDHASH")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("ROL")]
    public string Rol { get; set; } = string.Empty;

    // Por defecto Activa
    [Column("ACTIVO")]
    public bool Activo { get; set; }
}

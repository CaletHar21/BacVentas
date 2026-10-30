using System.Security.Claims;
using BacVentas.Web.Models;
using BacVentas.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BacVentas.Web.Controllers;

public class CuentaController : Controller
{
    private readonly ApiClient _api;

    public CuentaController(ApiClient api) => _api = api;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginVm { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]   // protege el formulario contra CSRF
    public async Task<IActionResult> Login(LoginVm model)
    {
        if (!ModelState.IsValid) return View(model);

        UsuarioSesion? usuario;
        try
        {
            // La API valida la contraseña con BCrypt; la Web nunca ve el hash.
            usuario = await _api.LoginAsync(model.Usuario, model.Password);
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }

        // Claims = datos del usuario que viajan dentro de la cookie cifrada.
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, usuario.Usuario),
            new(ClaimTypes.Role, usuario.Rol),            // usado por [Authorize(Roles = ...)]
            new("NombreCompleto", usuario.NombreCompleto) // se registra como vendedor
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));

        // Url.IsLocalUrl evita "open redirect" (que nos manden a un sitio externo).
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        // El operador va directo a registrar ventas; el admin al inicio.
        return usuario.Rol == "OPERADOR"
            ? RedirectToAction("Index", "Ventas")
            : RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccesoDenegado() => View();
}

using System.Globalization;
using BacVentas.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using QuestPDF.Infrastructure;

// QuestPDF es gratuito para uso personal/pequeñas empresas con la licencia Community.
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ---------- Cliente HTTP para consumir la API ----------
// AddHttpClient<ApiClient> registra un HttpClient administrado por ASP.NET
// (reutiliza conexiones; evita el problema de crear "new HttpClient()" en cada petición).
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? "https://localhost:7180/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    // Solo en desarrollo: acepta el certificado HTTPS local de la API.
    // En producción se usa un certificado válido y esta línea no aplica.
    if (builder.Environment.IsDevelopment())
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
});

// ---------- Autenticación con cookie + roles ----------
// Al iniciar sesión se guarda una cookie cifrada con el usuario y su rol (claims).
// [Authorize(Roles = "...")] en los controladores usa ese rol para permitir o negar el acceso.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";                 // sin sesión -> aquí
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado"; // sin permiso -> aquí
        options.LogoutPath = "/Cuenta/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;                    // se renueva mientras el usuario trabaja
        options.Cookie.Name = "BacVentas.Auth";
        options.Cookie.HttpOnly = true;                      // JavaScript no puede leer la cookie
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Cultura fija en-US: punto como separador decimal (El Salvador usa dólares y "0.75"),
// así el precio de los formularios se interpreta igual en cualquier PC.
var cultura = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = new[] { cultura },
    SupportedUICultures = new[] { cultura }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// El orden importa: primero saber QUIÉN es (Authentication) y luego QUÉ puede hacer (Authorization).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

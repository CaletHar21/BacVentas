# BacVentas

Sistema de ventas desarrollado en **.NET 8**: una **API REST** que gestiona productos, usuarios y ventas sobre **SQL Server** con procedimientos almacenados, y una **aplicación web MVC** que la consume.

## Arquitectura

```
Navegador ──► BacVentas.Web (ASP.NET Core MVC) ──► BacVentas.Api (ASP.NET Core Web API) ──► SQL Server (BACVENTASDB)
              login, vistas, reportes                 Entity Framework Core + procedimientos almacenados
```

- La Web nunca accede a la base de datos: todo pasa por la API mediante `HttpClient`.
- Todo acceso a datos usa procedimientos almacenados, ejecutados con Entity Framework Core y parámetros.

## Tecnologías

| Capa | Tecnología |
|---|---|
| API | ASP.NET Core Web API 8, Entity Framework Core 8, BCrypt.Net, Swagger |
| Web | ASP.NET Core MVC 8, Razor, Bootstrap 5, JavaScript, SweetAlert2 |
| Reportes | QuestPDF (PDF), ClosedXML (Excel) |
| Base de datos | SQL Server, procedimientos almacenados |

## Funcionalidades

- Inicio de sesión con usuario y contraseña; contraseñas almacenadas con hash **BCrypt**.
- Roles: **ADMIN** (productos, ventas y reportes) y **OPERADOR** (solo ventas).
- CRUD de productos con confirmación antes de eliminar.
- Registro de ventas: búsqueda de producto por código, cálculo en pantalla de subtotal, IVA (13 %) y total.
- La venta se guarda en una sola transacción; el precio se toma de la base de datos, no del navegador.
- Notificaciones de cada operación y manejo centralizado de errores.
- Reportes de ventas en **PDF** y **Excel**, con filtro por fechas.

## Requisitos

- .NET 8 SDK
- SQL Server (instancia local `localhost`)
- Visual Studio 2022 (17.8 o superior) o Visual Studio 2026

## Instalación

1. **Base de datos**: ejecutar `database/01_BACVENTASDB.sql` en SQL Server. Crea la base `BACVENTASDB`, las tablas, los procedimientos almacenados y productos de ejemplo.

   ```powershell
   sqlcmd -S localhost -E -C -f 65001 -i "database\01_BACVENTASDB.sql"
   ```

2. **Cadena de conexión**: si el servidor no es `localhost`, ajustar `ConnectionStrings:BacVentas` en `BacVentas.Api/appsettings.json`.

3. **Ejecutar**: abrir `BacVentas.sln`, ir a *Solución → Configurar proyectos de inicio → Proyectos de inicio múltiples*, marcar **BacVentas.Api** y **BacVentas.Web** en *Iniciar* (la API primero) y presionar **F5**.

   - API (Swagger): https://localhost:7180/swagger
   - Web: https://localhost:7072

   Al iniciar, la API crea los usuarios de prueba con su contraseña cifrada.

## Usuarios de prueba

| Usuario | Contraseña | Rol |
|---|---|---|
| admin | Admin123* | ADMIN |
| usuario1 | Operador123* | OPERADOR |

Para ver las contraseñas cifradas en la base de datos:

```sql
SELECT USUARIO, ROL, PASSWORDHASH FROM BACVENTASDB.dbo.USUARIOS;
```

## Endpoints de la API

| Método | Ruta | Descripción |
|---|---|---|
| POST | /api/auth/login | Inicio de sesión |
| GET | /api/productos | Listar productos |
| GET | /api/productos/{id} | Buscar producto por código |
| POST | /api/productos | Crear producto |
| PUT | /api/productos/{id} | Actualizar producto |
| DELETE | /api/productos/{id} | Eliminar producto |
| POST | /api/ventas | Registrar venta con su detalle |
| GET | /api/ventas?desde=&hasta= | Listar ventas (reportes) |
| GET | /api/ventas/{id}/detalle | Detalle de una venta |

## Estructura

```
BacVentas.sln
├── BacVentas.Api/        Controllers, Models, Data (DbContext), Middleware
├── BacVentas.Web/        Controllers, Views, Services (ApiClient), wwwroot/js
├── database/             01_BACVENTASDB.sql
└── docs/                 DICCIONARIO_DATOS.md
```

El diccionario de datos completo está en [`docs/DICCIONARIO_DATOS.md`](docs/DICCIONARIO_DATOS.md) y también puede consultarse desde la base de datos con `EXEC dbo.SP_DICCIONARIO_DATOS;`.

---

Desarrollado por Caleth Ramírez.

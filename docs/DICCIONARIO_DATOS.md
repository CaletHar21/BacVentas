# DICCIONARIO DE DATOS — BACVENTASDB

**Motor:** SQL Server
**Base de datos:** `BACVENTASDB`
**Convención:** nombres de tablas, campos y procedimientos en MAYÚSCULAS; campos sin separadores.
**Script:** `database/01_BACVENTASDB.sql`

> El diccionario también está guardado dentro de la base de datos (descripciones `MS_Description`) y se puede consultar con:
> ```sql
> EXEC dbo.SP_DICCIONARIO_DATOS;
> ```

## Relaciones

```
USUARIOS        (independiente: login y roles)

PRODUCTOS 1 ──< N DETALLEVENTAS N >── 1 VENTAS
   IDPRO              IDPRO / IDVENTA        IDVENTA
```

---

## 1. USUARIOS
Usuarios con acceso al sistema y su rol.

| # | CAMPO | TIPO | NULO | LLAVE | DESCRIPCIÓN |
|---|---|---|---|---|---|
| 1 | IDUSUARIO | INT IDENTITY | NO | PK | Identificador único del usuario (autoincremental). |
| 2 | USUARIO | NVARCHAR(50) | NO | UQ | Nombre de usuario para iniciar sesión (único). |
| 3 | NOMBRECOMPLETO | NVARCHAR(100) | NO | | Nombre completo; se registra como vendedor en las ventas. |
| 4 | PASSWORDHASH | NVARCHAR(100) | NO | | Contraseña cifrada con BCrypt (nunca en texto plano). |
| 5 | ROL | NVARCHAR(20) | NO | CK | `ADMIN` (acceso total) u `OPERADOR` (solo ventas). |
| 6 | ACTIVO | BIT | NO | DF = 1 | Indica si el usuario puede iniciar sesión. |

## 2. PRODUCTOS
Inventario de productos disponibles para la venta.

| # | CAMPO | TIPO | NULO | LLAVE | DESCRIPCIÓN |
|---|---|---|---|---|---|
| 1 | IDPRO | INT IDENTITY | NO | PK | Código del producto; se usa para buscarlo en ventas. |
| 2 | PRODUCTO | NVARCHAR(150) | NO | | Nombre o descripción del producto. |
| 3 | PRECIO | DECIMAL(10,2) | NO | CK > 0 | Precio unitario sin IVA. |

## 3. VENTAS
Encabezado de cada venta realizada.

| # | CAMPO | TIPO | NULO | LLAVE | DESCRIPCIÓN |
|---|---|---|---|---|---|
| 1 | IDVENTA | INT IDENTITY | NO | PK | Número de venta (autoincremental). |
| 2 | FECHA | DATETIME2(0) | NO | DF = SYSDATETIME() | Fecha y hora en que se registró la venta. |
| 3 | VENDEDOR | NVARCHAR(100) | NO | | Nombre del usuario que realizó la venta. |
| 4 | TOTAL | DECIMAL(12,2) | NO | | Total a pagar (subtotal + IVA). |

## 4. DETALLEVENTAS
Productos incluidos en cada venta.

| # | CAMPO | TIPO | NULO | LLAVE | DESCRIPCIÓN |
|---|---|---|---|---|---|
| 1 | IDDE | INT IDENTITY | NO | PK | Identificador de la línea de detalle. |
| 2 | FECHA | DATETIME2(0) | NO | | Fecha de la venta a la que pertenece la línea. |
| 3 | IDVENTA | INT | NO | FK → VENTAS | Venta a la que pertenece la línea. |
| 4 | IDPRO | INT | NO | FK → PRODUCTOS | Producto vendido. |
| 5 | CANTIDAD | INT | NO | CK > 0 | Cantidad vendida. |
| 6 | PRECIO | DECIMAL(10,2) | NO | | Precio unitario al momento de la venta. |
| 7 | IVA | DECIMAL(10,2) | NO | | 13% de CANTIDAD × PRECIO, redondeado a 2 decimales. |
| 8 | TOTAL | DECIMAL(12,2) | NO | | CANTIDAD × PRECIO + IVA. |

---

## Procedimientos almacenados

| PROCEDIMIENTO | PARÁMETROS | DESCRIPCIÓN |
|---|---|---|
| SP_PRODUCTOS_LISTAR | — | Lista todos los productos. |
| SP_PRODUCTOS_OBTENERPORID | @IDPRO | Busca un producto por su código. |
| SP_PRODUCTOS_INSERTAR | @PRODUCTO, @PRECIO | Crea un producto y lo devuelve. |
| SP_PRODUCTOS_ACTUALIZAR | @IDPRO, @PRODUCTO, @PRECIO | Actualiza un producto; error 50001 si no existe. |
| SP_PRODUCTOS_ELIMINAR | @IDPRO | Elimina un producto; error 50002 si tiene ventas. |
| SP_VENTAS_REGISTRAR | @VENDEDOR, @DETALLE (JSON) | Registra encabezado y detalle en una sola transacción. Toma el precio de la BD y calcula IVA y totales. |
| SP_VENTAS_LISTAR | @DESDE, @HASTA (opcionales) | Lista ventas con subtotal, IVA y total (para reportes). |
| SP_VENTAS_DETALLE | @IDVENTA | Detalle de una venta con nombre de producto. |
| SP_USUARIOS_OBTENERPORUSUARIO | @USUARIO | Obtiene un usuario activo para validar el login. |
| SP_USUARIOS_INSERTAR | @USUARIO, @NOMBRECOMPLETO, @PASSWORDHASH, @ROL | Crea un usuario; error 50020 si ya existe. |
| SP_DICCIONARIO_DATOS | — | Muestra este diccionario desde la propia base de datos. |

## Códigos de error de negocio

| CÓDIGO | MENSAJE |
|---|---|
| 50001 | El producto no existe. |
| 50002 | No se puede eliminar: el producto tiene ventas registradas. |
| 50010 | El detalle de la venta no es válido. |
| 50011 | La venta no tiene productos. |
| 50012 | Uno o más productos no existen. |
| 50013 | La cantidad debe ser mayor a cero. |
| 50020 | El usuario ya existe. |

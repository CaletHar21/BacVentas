/* =====================================================================
   BACVENTASDB - PRUEBA TÉCNICA
   TABLAS: USUARIOS, PRODUCTOS, VENTAS, DETALLEVENTAS
   Convención: nombres en MAYÚSCULAS, campos sin separadores.
   El script elimina y recrea la base de datos (se puede re-ejecutar).
   ===================================================================== */
USE master;
GO
IF DB_ID('BACVENTASDB') IS NOT NULL
BEGIN
    ALTER DATABASE BACVENTASDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BACVENTASDB;
END
GO
CREATE DATABASE BACVENTASDB;
GO
USE BACVENTASDB;
GO

/* =====================================================================
   TABLAS
   ===================================================================== */
CREATE TABLE dbo.USUARIOS (
    IDUSUARIO      INT IDENTITY(1,1) CONSTRAINT PK_USUARIOS PRIMARY KEY,
    USUARIO        NVARCHAR(50)  NOT NULL CONSTRAINT UQ_USUARIOS_USUARIO UNIQUE,
    NOMBRECOMPLETO NVARCHAR(100) NOT NULL,
    PASSWORDHASH   NVARCHAR(100) NOT NULL,
    ROL            NVARCHAR(20)  NOT NULL CONSTRAINT CK_USUARIOS_ROL CHECK (ROL IN ('ADMIN','OPERADOR')),
    ACTIVO         BIT NOT NULL CONSTRAINT DF_USUARIOS_ACTIVO DEFAULT 1
);

CREATE TABLE dbo.PRODUCTOS (
    IDPRO    INT IDENTITY(1,1) CONSTRAINT PK_PRODUCTOS PRIMARY KEY,
    PRODUCTO NVARCHAR(150) NOT NULL,
    PRECIO   DECIMAL(10,2) NOT NULL CONSTRAINT CK_PRODUCTOS_PRECIO CHECK (PRECIO > 0)
);

CREATE TABLE dbo.VENTAS (
    IDVENTA  INT IDENTITY(1,1) CONSTRAINT PK_VENTAS PRIMARY KEY,
    FECHA    DATETIME2(0)  NOT NULL CONSTRAINT DF_VENTAS_FECHA DEFAULT SYSDATETIME(),
    VENDEDOR NVARCHAR(100) NOT NULL,
    TOTAL    DECIMAL(12,2) NOT NULL
);

CREATE TABLE dbo.DETALLEVENTAS (
    IDDE     INT IDENTITY(1,1) CONSTRAINT PK_DETALLEVENTAS PRIMARY KEY,
    FECHA    DATETIME2(0)  NOT NULL,
    IDVENTA  INT NOT NULL CONSTRAINT FK_DETALLEVENTAS_VENTAS    FOREIGN KEY REFERENCES dbo.VENTAS(IDVENTA),
    IDPRO    INT NOT NULL CONSTRAINT FK_DETALLEVENTAS_PRODUCTOS FOREIGN KEY REFERENCES dbo.PRODUCTOS(IDPRO),
    CANTIDAD INT NOT NULL CONSTRAINT CK_DETALLEVENTAS_CANTIDAD CHECK (CANTIDAD > 0),
    PRECIO   DECIMAL(10,2) NOT NULL,
    IVA      DECIMAL(10,2) NOT NULL,
    TOTAL    DECIMAL(12,2) NOT NULL
);
GO

/* =====================================================================
   DICCIONARIO DE DATOS (descripciones guardadas en la propia BD)
   ===================================================================== */
-- USUARIOS
EXEC sp_addextendedproperty N'MS_Description', N'Usuarios con acceso al sistema y su rol.', N'SCHEMA', N'dbo', N'TABLE', N'USUARIOS';
EXEC sp_addextendedproperty N'MS_Description', N'Identificador único del usuario (autoincremental).', N'SCHEMA', N'dbo', N'TABLE', N'USUARIOS', N'COLUMN', N'IDUSUARIO';
EXEC sp_addextendedproperty N'MS_Description', N'Nombre de usuario para iniciar sesión (único).', N'SCHEMA', N'dbo', N'TABLE', N'USUARIOS', N'COLUMN', N'USUARIO';
EXEC sp_addextendedproperty N'MS_Description', N'Nombre completo; se registra como vendedor en las ventas.', N'SCHEMA', N'dbo', N'TABLE', N'USUARIOS', N'COLUMN', N'NOMBRECOMPLETO';
EXEC sp_addextendedproperty N'MS_Description', N'Contraseña cifrada con BCrypt (nunca en texto plano).', N'SCHEMA', N'dbo', N'TABLE', N'USUARIOS', N'COLUMN', N'PASSWORDHASH';
EXEC sp_addextendedproperty N'MS_Description', N'Rol del usuario: ADMIN (acceso total) u OPERADOR (solo ventas).', N'SCHEMA', N'dbo', N'TABLE', N'USUARIOS', N'COLUMN', N'ROL';
EXEC sp_addextendedproperty N'MS_Description', N'Indica si el usuario puede iniciar sesión (1 = activo).', N'SCHEMA', N'dbo', N'TABLE', N'USUARIOS', N'COLUMN', N'ACTIVO';
-- PRODUCTOS
EXEC sp_addextendedproperty N'MS_Description', N'Inventario de productos disponibles para la venta.', N'SCHEMA', N'dbo', N'TABLE', N'PRODUCTOS';
EXEC sp_addextendedproperty N'MS_Description', N'Código del producto (autoincremental); se usa para buscarlo en ventas.', N'SCHEMA', N'dbo', N'TABLE', N'PRODUCTOS', N'COLUMN', N'IDPRO';
EXEC sp_addextendedproperty N'MS_Description', N'Nombre o descripción del producto.', N'SCHEMA', N'dbo', N'TABLE', N'PRODUCTOS', N'COLUMN', N'PRODUCTO';
EXEC sp_addextendedproperty N'MS_Description', N'Precio unitario sin IVA (debe ser mayor a 0).', N'SCHEMA', N'dbo', N'TABLE', N'PRODUCTOS', N'COLUMN', N'PRECIO';
-- VENTAS
EXEC sp_addextendedproperty N'MS_Description', N'Encabezado de cada venta realizada.', N'SCHEMA', N'dbo', N'TABLE', N'VENTAS';
EXEC sp_addextendedproperty N'MS_Description', N'Número de venta (autoincremental).', N'SCHEMA', N'dbo', N'TABLE', N'VENTAS', N'COLUMN', N'IDVENTA';
EXEC sp_addextendedproperty N'MS_Description', N'Fecha y hora en que se registró la venta.', N'SCHEMA', N'dbo', N'TABLE', N'VENTAS', N'COLUMN', N'FECHA';
EXEC sp_addextendedproperty N'MS_Description', N'Nombre del usuario que realizó la venta.', N'SCHEMA', N'dbo', N'TABLE', N'VENTAS', N'COLUMN', N'VENDEDOR';
EXEC sp_addextendedproperty N'MS_Description', N'Total a pagar de la venta (subtotal + IVA).', N'SCHEMA', N'dbo', N'TABLE', N'VENTAS', N'COLUMN', N'TOTAL';
-- DETALLEVENTAS
EXEC sp_addextendedproperty N'MS_Description', N'Productos incluidos en cada venta.', N'SCHEMA', N'dbo', N'TABLE', N'DETALLEVENTAS';
EXEC sp_addextendedproperty N'MS_Description', N'Identificador de la línea de detalle (autoincremental).', N'SCHEMA', N'dbo', N'TABLE', N'DETALLEVENTAS', N'COLUMN', N'IDDE';
EXEC sp_addextendedproperty N'MS_Description', N'Fecha de la venta a la que pertenece la línea.', N'SCHEMA', N'dbo', N'TABLE', N'DETALLEVENTAS', N'COLUMN', N'FECHA';
EXEC sp_addextendedproperty N'MS_Description', N'Venta a la que pertenece la línea (FK a VENTAS).', N'SCHEMA', N'dbo', N'TABLE', N'DETALLEVENTAS', N'COLUMN', N'IDVENTA';
EXEC sp_addextendedproperty N'MS_Description', N'Producto vendido (FK a PRODUCTOS).', N'SCHEMA', N'dbo', N'TABLE', N'DETALLEVENTAS', N'COLUMN', N'IDPRO';
EXEC sp_addextendedproperty N'MS_Description', N'Cantidad vendida (mayor a 0).', N'SCHEMA', N'dbo', N'TABLE', N'DETALLEVENTAS', N'COLUMN', N'CANTIDAD';
EXEC sp_addextendedproperty N'MS_Description', N'Precio unitario al momento de la venta.', N'SCHEMA', N'dbo', N'TABLE', N'DETALLEVENTAS', N'COLUMN', N'PRECIO';
EXEC sp_addextendedproperty N'MS_Description', N'IVA de la línea (13% de CANTIDAD x PRECIO).', N'SCHEMA', N'dbo', N'TABLE', N'DETALLEVENTAS', N'COLUMN', N'IVA';
EXEC sp_addextendedproperty N'MS_Description', N'Total de la línea (CANTIDAD x PRECIO + IVA).', N'SCHEMA', N'dbo', N'TABLE', N'DETALLEVENTAS', N'COLUMN', N'TOTAL';
GO

/* =====================================================================
   PROCEDIMIENTOS ALMACENADOS - PRODUCTOS
   ===================================================================== */
CREATE OR ALTER PROCEDURE dbo.SP_PRODUCTOS_LISTAR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IDPRO, PRODUCTO, PRECIO FROM dbo.PRODUCTOS ORDER BY IDPRO;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PRODUCTOS_OBTENERPORID
    @IDPRO INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IDPRO, PRODUCTO, PRECIO FROM dbo.PRODUCTOS WHERE IDPRO = @IDPRO;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PRODUCTOS_INSERTAR
    @PRODUCTO NVARCHAR(150),
    @PRECIO   DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.PRODUCTOS (PRODUCTO, PRECIO) VALUES (@PRODUCTO, @PRECIO);
    SELECT IDPRO, PRODUCTO, PRECIO FROM dbo.PRODUCTOS WHERE IDPRO = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PRODUCTOS_ACTUALIZAR
    @IDPRO    INT,
    @PRODUCTO NVARCHAR(150),
    @PRECIO   DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PRODUCTOS SET PRODUCTO = @PRODUCTO, PRECIO = @PRECIO WHERE IDPRO = @IDPRO;
    IF @@ROWCOUNT = 0
        THROW 50001, N'El producto no existe.', 1;
    SELECT IDPRO, PRODUCTO, PRECIO FROM dbo.PRODUCTOS WHERE IDPRO = @IDPRO;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_PRODUCTOS_ELIMINAR
    @IDPRO INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.PRODUCTOS WHERE IDPRO = @IDPRO)
        THROW 50001, N'El producto no existe.', 1;
    IF EXISTS (SELECT 1 FROM dbo.DETALLEVENTAS WHERE IDPRO = @IDPRO)
        THROW 50002, N'No se puede eliminar: el producto tiene ventas registradas.', 1;
    DELETE FROM dbo.PRODUCTOS WHERE IDPRO = @IDPRO;
END
GO

/* =====================================================================
   PROCEDIMIENTOS ALMACENADOS - VENTAS
   El detalle llega como JSON: [{"IDPRO":1,"CANTIDAD":2}, ...]
   El precio se toma de la BD (no se confía en el cliente).
   IVA 13% calculado por línea. Todo en una sola transacción.
   ===================================================================== */
CREATE OR ALTER PROCEDURE dbo.SP_VENTAS_REGISTRAR
    @VENDEDOR NVARCHAR(100),
    @DETALLE  NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;   -- si algo falla, se revierte todo

    DECLARE @FECHA DATETIME2(0) = SYSDATETIME();
    DECLARE @TASAIVA DECIMAL(5,2) = 0.13;
    DECLARE @ITEMS TABLE (IDPRO INT, CANTIDAD INT, PRECIO DECIMAL(10,2), IVA DECIMAL(10,2), TOTAL DECIMAL(12,2));

    IF @DETALLE IS NULL OR ISJSON(@DETALLE) = 0
        THROW 50010, N'El detalle de la venta no es válido.', 1;

    DECLARE @LINEAS INT = (SELECT COUNT(*) FROM OPENJSON(@DETALLE));
    IF @LINEAS = 0
        THROW 50011, N'La venta no tiene productos.', 1;

    INSERT INTO @ITEMS (IDPRO, CANTIDAD, PRECIO, IVA, TOTAL)
    SELECT J.IDPRO,
           J.CANTIDAD,
           P.PRECIO,
           ROUND(J.CANTIDAD * P.PRECIO * @TASAIVA, 2),
           ROUND(J.CANTIDAD * P.PRECIO, 2) + ROUND(J.CANTIDAD * P.PRECIO * @TASAIVA, 2)
    FROM OPENJSON(@DETALLE) WITH (IDPRO INT '$.IDPRO', CANTIDAD INT '$.CANTIDAD') AS J
    INNER JOIN dbo.PRODUCTOS P ON P.IDPRO = J.IDPRO;

    IF (SELECT COUNT(*) FROM @ITEMS) <> @LINEAS
        THROW 50012, N'Uno o más productos no existen.', 1;
    IF EXISTS (SELECT 1 FROM @ITEMS WHERE CANTIDAD IS NULL OR CANTIDAD <= 0)
        THROW 50013, N'La cantidad debe ser mayor a cero.', 1;

    BEGIN TRANSACTION;
        INSERT INTO dbo.VENTAS (FECHA, VENDEDOR, TOTAL)
        SELECT @FECHA, @VENDEDOR, SUM(TOTAL) FROM @ITEMS;

        DECLARE @IDVENTA INT = SCOPE_IDENTITY();

        INSERT INTO dbo.DETALLEVENTAS (FECHA, IDVENTA, IDPRO, CANTIDAD, PRECIO, IVA, TOTAL)
        SELECT @FECHA, @IDVENTA, IDPRO, CANTIDAD, PRECIO, IVA, TOTAL FROM @ITEMS;
    COMMIT TRANSACTION;

    SELECT IDVENTA, FECHA, VENDEDOR, TOTAL FROM dbo.VENTAS WHERE IDVENTA = @IDVENTA;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_VENTAS_LISTAR
    @DESDE DATE = NULL,
    @HASTA DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT V.IDVENTA,
           V.FECHA,
           V.VENDEDOR,
           CAST(V.TOTAL - ISNULL(SUM(D.IVA), 0) AS DECIMAL(12,2)) AS SUBTOTAL,
           CAST(ISNULL(SUM(D.IVA), 0) AS DECIMAL(12,2))           AS IVA,
           V.TOTAL
    FROM dbo.VENTAS V
    LEFT JOIN dbo.DETALLEVENTAS D ON D.IDVENTA = V.IDVENTA
    WHERE (@DESDE IS NULL OR V.FECHA >= @DESDE)
      AND (@HASTA IS NULL OR V.FECHA < DATEADD(DAY, 1, @HASTA))
    GROUP BY V.IDVENTA, V.FECHA, V.VENDEDOR, V.TOTAL
    ORDER BY V.IDVENTA DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_VENTAS_DETALLE
    @IDVENTA INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT D.IDDE, D.FECHA, D.IDVENTA, D.IDPRO, P.PRODUCTO, D.CANTIDAD, D.PRECIO, D.IVA, D.TOTAL
    FROM dbo.DETALLEVENTAS D
    INNER JOIN dbo.PRODUCTOS P ON P.IDPRO = D.IDPRO
    WHERE D.IDVENTA = @IDVENTA
    ORDER BY D.IDDE;
END
GO

/* =====================================================================
   PROCEDIMIENTOS ALMACENADOS - USUARIOS
   ===================================================================== */
CREATE OR ALTER PROCEDURE dbo.SP_USUARIOS_OBTENERPORUSUARIO
    @USUARIO NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IDUSUARIO, USUARIO, NOMBRECOMPLETO, PASSWORDHASH, ROL, ACTIVO
    FROM dbo.USUARIOS
    WHERE USUARIO = @USUARIO AND ACTIVO = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_USUARIOS_INSERTAR
    @USUARIO        NVARCHAR(50),
    @NOMBRECOMPLETO NVARCHAR(100),
    @PASSWORDHASH   NVARCHAR(100),
    @ROL            NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.USUARIOS WHERE USUARIO = @USUARIO)
        THROW 50020, N'El usuario ya existe.', 1;
    INSERT INTO dbo.USUARIOS (USUARIO, NOMBRECOMPLETO, PASSWORDHASH, ROL)
    VALUES (@USUARIO, @NOMBRECOMPLETO, @PASSWORDHASH, @ROL);
END
GO

/* =====================================================================
   PROCEDIMIENTO - DICCIONARIO DE DATOS
   Muestra tablas, campos, tipos, llaves y descripciones.
   ===================================================================== */
CREATE OR ALTER PROCEDURE dbo.SP_DICCIONARIO_DATOS
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  T.name                                AS TABLA,
            CAST(TP.value AS NVARCHAR(500))       AS DESCRIPCIONTABLA,
            C.column_id                           AS ORDEN,
            C.name                                AS CAMPO,
            UPPER(TY.name) +
                CASE
                    WHEN TY.name IN ('nvarchar','nchar')  THEN '(' + IIF(C.max_length = -1, 'MAX', CAST(C.max_length / 2 AS VARCHAR(10))) + ')'
                    WHEN TY.name IN ('varchar','char')    THEN '(' + IIF(C.max_length = -1, 'MAX', CAST(C.max_length AS VARCHAR(10))) + ')'
                    WHEN TY.name IN ('decimal','numeric') THEN '(' + CAST(C.precision AS VARCHAR(5)) + ',' + CAST(C.scale AS VARCHAR(5)) + ')'
                    WHEN TY.name = 'datetime2'            THEN '(' + CAST(C.scale AS VARCHAR(5)) + ')'
                    ELSE ''
                END                               AS TIPODATO,
            IIF(C.is_nullable = 1, 'SI', 'NO')    AS PERMITENULO,
            IIF(C.is_identity = 1, 'SI', 'NO')    AS AUTOINCREMENTAL,
            IIF(PK.column_id IS NULL, '', 'PK')
              + IIF(FK.referenced_object_id IS NULL, '',
                    IIF(PK.column_id IS NULL, '', ' / ') + 'FK -> ' + OBJECT_NAME(FK.referenced_object_id)) AS LLAVE,
            CAST(CP.value AS NVARCHAR(500))       AS DESCRIPCION
    FROM sys.tables T
    INNER JOIN sys.columns C ON C.object_id = T.object_id
    INNER JOIN sys.types TY  ON TY.user_type_id = C.user_type_id
    LEFT JOIN sys.extended_properties TP
           ON TP.class = 1 AND TP.major_id = T.object_id AND TP.minor_id = 0 AND TP.name = 'MS_Description'
    LEFT JOIN sys.extended_properties CP
           ON CP.class = 1 AND CP.major_id = T.object_id AND CP.minor_id = C.column_id AND CP.name = 'MS_Description'
    LEFT JOIN (SELECT IC.object_id, IC.column_id
               FROM sys.indexes I
               INNER JOIN sys.index_columns IC ON IC.object_id = I.object_id AND IC.index_id = I.index_id
               WHERE I.is_primary_key = 1) PK
           ON PK.object_id = C.object_id AND PK.column_id = C.column_id
    LEFT JOIN sys.foreign_key_columns FK
           ON FK.parent_object_id = C.object_id AND FK.parent_column_id = C.column_id
    ORDER BY T.name, C.column_id;
END
GO

/* =====================================================================
   DATOS DE PRUEBA
   (los usuarios los crea la API al iniciar, con la contraseña
    cifrada con BCrypt desde C#)
   ===================================================================== */
INSERT INTO dbo.PRODUCTOS (PRODUCTO, PRECIO) VALUES
(N'LAPICERO BIC COLOR AZUL', 0.75),
(N'CUADERNO CUADRICULADO',   3.50),
(N'CAJA DE COLORES FACELA',  4.10),
(N'BORRADOR BLANCO',         0.25),
(N'REGLA DE 30 CM',          0.60);
GO

PRINT N'BACVENTASDB creada correctamente.';
GO

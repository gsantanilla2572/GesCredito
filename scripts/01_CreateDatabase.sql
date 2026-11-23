/* =====================================================================
   GesCrédito - Creación de base de datos, tablas y datos de prueba
   Idempotente: se puede ejecutar varias veces sin duplicar nada.
   Ejecutar con SSMS / Azure Data Studio / sqlcmd (ver docs/GUIA_PASO_A_PASO.md)
   ===================================================================== */
IF DB_ID(N'GesCreditoDb') IS NULL
    CREATE DATABASE GesCreditoDb;
GO
USE GesCreditoDb;
GO

/* ---------- Tabla Clientes ---------- */
IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
        Identificacion NVARCHAR(20)  NOT NULL,
        Nombres        NVARCHAR(100) NOT NULL,
        Apellidos      NVARCHAR(100) NOT NULL,
        Email          NVARCHAR(150) NOT NULL,
        Telefono       NVARCHAR(30)  NULL,
        Direccion      NVARCHAR(200) NULL,
        FechaRegistro  DATETIME2     NOT NULL CONSTRAINT DF_Clientes_FechaRegistro DEFAULT (SYSUTCDATETIME()),
        Activo         BIT           NOT NULL CONSTRAINT DF_Clientes_Activo DEFAULT (1)
    );
    CREATE UNIQUE INDEX UX_Clientes_Identificacion ON dbo.Clientes (Identificacion);
END
GO

/* ---------- Tabla Creditos ----------
   Estado: 1=Solicitado 2=Aprobado 3=Desembolsado 4=Pagado 5=Rechazado 6=Cancelado */
IF OBJECT_ID(N'dbo.Creditos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Creditos
    (
        Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Creditos PRIMARY KEY,
        Numero           NVARCHAR(30)  NOT NULL,
        ClienteId        INT           NOT NULL,
        Monto            DECIMAL(18,2) NOT NULL,
        TasaInteresAnual DECIMAL(9,4)  NOT NULL,
        PlazoMeses       INT           NOT NULL,
        CuotaMensual     DECIMAL(18,2) NOT NULL,
        Estado           TINYINT       NOT NULL CONSTRAINT DF_Creditos_Estado DEFAULT (1),
        FechaSolicitud   DATETIME2     NOT NULL CONSTRAINT DF_Creditos_FechaSolicitud DEFAULT (SYSUTCDATETIME()),
        FechaDesembolso  DATETIME2     NULL,
        Observaciones    NVARCHAR(500) NULL,
        CONSTRAINT FK_Creditos_Clientes FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes (Id),
        CONSTRAINT CK_Creditos_Monto  CHECK (Monto > 0),
        CONSTRAINT CK_Creditos_Tasa   CHECK (TasaInteresAnual >= 0 AND TasaInteresAnual <= 100),
        CONSTRAINT CK_Creditos_Plazo  CHECK (PlazoMeses BETWEEN 1 AND 360),
        CONSTRAINT CK_Creditos_Estado CHECK (Estado BETWEEN 1 AND 6)
    );
    CREATE UNIQUE INDEX UX_Creditos_Numero ON dbo.Creditos (Numero);
    CREATE INDEX IX_Creditos_Cliente_Estado ON dbo.Creditos (ClienteId, Estado);
END
GO

/* ---------- Datos de prueba (inventados) ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes)
BEGIN
    INSERT INTO dbo.Clientes (Identificacion, Nombres, Apellidos, Email, Telefono, Direccion, Activo) VALUES
    (N'1010101010', N'Carlos Andrés', N'Ramírez Soto',   N'carlos.ramirez@example.com', N'3001112233', N'Cra 7 # 45-12, Bogotá',      1),
    (N'1020202020', N'María Fernanda', N'López Gómez',   N'maria.lopez@example.com',    N'3012223344', N'Calle 80 # 20-30, Bogotá',    1),
    (N'1030303030', N'Juan Pablo',     N'Martínez Ruiz', N'juan.martinez@example.com',  N'3023334455', N'Av. El Poblado # 10-5, Medellín', 1),
    (N'1040404040', N'Laura Camila',   N'Pérez Díaz',    N'laura.perez@example.com',    N'3034445566', N'Calle 5 # 38-10, Cali',       1),
    (N'1050505050', N'Andrés Felipe',  N'Torres Vega',   N'andres.torres@example.com',  N'3045556677', N'Cra 50 # 70-22, Barranquilla',1),
    (N'1060606060', N'Diana Marcela',  N'Castro Rey',    N'diana.castro@example.com',   N'3056667788', N'Calle 100 # 15-40, Bogotá',   1),
    (N'1070707070', N'Sergio Iván',    N'Mora Pardo',    N'sergio.mora@example.com',    N'3067778899', N'Cra 15 # 85-33, Bogotá',      1),
    (N'1080808080', N'Paola Andrea',   N'Rojas Núñez',   N'paola.rojas@example.com',    NULL,          NULL,                           1),
    (N'1090909090', N'Hernán Darío',   N'Vargas León',   N'hernan.vargas@example.com',  N'3089990011', N'Calle 22 # 9-14, Bucaramanga',0),
    (N'1101010101', N'Natalia',        N'Suárez Mejía',  N'natalia.suarez@example.com', N'3091011122', N'Cra 43A # 1-50, Medellín',    1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Creditos)
BEGIN
    /* Cuota mensual por sistema francés: M*r / (1 - (1+r)^-n), r = tasa/12/100 */
    INSERT INTO dbo.Creditos (Numero, ClienteId, Monto, TasaInteresAnual, PlazoMeses, CuotaMensual, Estado, FechaSolicitud, FechaDesembolso, Observaciones)
    SELECT v.Numero, c.Id,
           CAST(v.Monto AS DECIMAL(18,2)), CAST(v.Tasa AS DECIMAL(9,4)), v.Plazo,
           CAST(ROUND(CASE WHEN v.Tasa = 0
                           THEN CAST(v.Monto AS FLOAT) / v.Plazo
                           ELSE CAST(v.Monto AS FLOAT) * (CAST(v.Tasa AS FLOAT)/1200.0)
                                / (1.0 - POWER(1.0 + CAST(v.Tasa AS FLOAT)/1200.0, -v.Plazo))
                      END, 2) AS DECIMAL(18,2)),
           v.Estado,
           DATEADD(DAY, -v.DiasAtras, SYSUTCDATETIME()),
           CASE WHEN v.Estado IN (3,4) THEN DATEADD(DAY, -v.DiasAtras + 3, SYSUTCDATETIME()) END,
           v.Obs
    FROM (VALUES
        (N'1010101010', N'CR-20260101-A00001',  15000000, 14.50, 36, 3, 300, N'Crédito de libre inversión'),
        (N'1010101010', N'CR-20260301-A00002',   5000000, 18.00, 12, 4, 240, N'Pagado en su totalidad'),
        (N'1020202020', N'CR-20260115-A00003',  80000000, 11.20, 120, 3, 280, N'Crédito de vivienda'),
        (N'1020202020', N'CR-20261001-A00004',   3000000, 21.00,  6, 1,  40, NULL),
        (N'1030303030', N'CR-20260210-A00005',  25000000, 15.75, 48, 3, 270, N'Compra de vehículo'),
        (N'1030303030', N'CR-20261015-A00006',  12000000, 16.00, 24, 2,  35, N'Aprobado, pendiente de desembolso'),
        (N'1040404040', N'CR-20260320-A00007',   8000000, 19.50, 18, 3, 200, N'Capital de trabajo'),
        (N'1040404040', N'CR-20260801-A00008',   2000000,  0.00, 10, 5,  90, N'Rechazado por capacidad de pago'),
        (N'1050505050', N'CR-20260405-A00009',  40000000, 12.80, 60, 3, 220, N'Remodelación'),
        (N'1050505050', N'CR-20261101-A00010',   6000000, 17.25, 12, 1,  15, NULL),
        (N'1060606060', N'CR-20260512-A00011',  18000000, 14.00, 36, 3, 190, NULL),
        (N'1060606060', N'CR-20260901-A00012',   4500000, 20.00,  9, 6,  70, N'Cancelado por el cliente'),
        (N'1070707070', N'CR-20260622-A00013',  30000000, 13.40, 72, 3, 160, N'Educación'),
        (N'1070707070', N'CR-20261110-A00014',   7000000, 18.50, 24, 1,  10, NULL),
        (N'1080808080', N'CR-20260715-A00015',   9500000, 16.80, 24, 3, 130, NULL),
        (N'1090909090', N'CR-20250101-A00016',  10000000, 15.00, 12, 4, 600, N'Cliente hoy inactivo'),
        (N'1101010101', N'CR-20260818-A00017',  22000000, 12.00, 48, 3, 100, N'Compra de maquinaria'),
        (N'1101010101', N'CR-20261112-A00018',   1500000,  0.00,  6, 1,   8, N'Promoción tasa 0 %')
    ) AS v(Identificacion, Numero, Monto, Tasa, Plazo, Estado, DiasAtras, Obs)
    JOIN dbo.Clientes c ON c.Identificacion = v.Identificacion;
END
GO

SELECT (SELECT COUNT(*) FROM dbo.Clientes) AS Clientes, (SELECT COUNT(*) FROM dbo.Creditos) AS Creditos;
GO

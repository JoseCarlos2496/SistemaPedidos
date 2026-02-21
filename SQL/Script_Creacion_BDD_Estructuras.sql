-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'SistemaPedido')
BEGIN
    CREATE DATABASE SistemaPedido;
END
GO

USE SistemaPedido;
GO

-- Tabla: PedidoCabecera
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PedidoCabecera' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PedidoCabecera (
        Id         INT           IDENTITY(1,1)  NOT NULL,
        ClienteId  INT                          NOT NULL,
        Fecha      DATETIME                     NOT NULL  DEFAULT GETDATE(),
        Total      DECIMAL(18, 2)               NOT NULL  DEFAULT 0,
        Usuario    NVARCHAR(100)                NOT NULL,
        CONSTRAINT PK_PedidoCabecera PRIMARY KEY (Id)
    );
    PRINT 'Tabla PedidoCabecera creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla PedidoCabecera ya existe.';
END
GO

-- Tabla: PedidoDetalle
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PedidoDetalle' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PedidoDetalle (
        Id          INT           IDENTITY(1,1)  NOT NULL,
        PedidoId    INT                          NOT NULL,
        ProductoId  INT                          NOT NULL,
        Cantidad    INT                          NOT NULL  DEFAULT 1,
        Precio      DECIMAL(18, 2)               NOT NULL  DEFAULT 0,
        CONSTRAINT PK_PedidoDetalle  PRIMARY KEY (Id),
        CONSTRAINT FK_PedidoDetalle_PedidoCabecera
            FOREIGN KEY (PedidoId) REFERENCES dbo.PedidoCabecera(Id)
    );
    PRINT 'Tabla PedidoDetalle creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla PedidoDetalle ya existe.';
END
GO

-- Tabla: LogAuditoria
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LogAuditoria' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.LogAuditoria (
        Id           INT           IDENTITY(1,1)  NOT NULL,
        Fecha        DATETIME                     NOT NULL  DEFAULT GETDATE(),
        Evento       NVARCHAR(200)                NOT NULL,
        Descripcion  NVARCHAR(MAX)                NULL,
        CONSTRAINT PK_LogAuditoria PRIMARY KEY (Id)
    );
    PRINT 'Tabla LogAuditoria creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla LogAuditoria ya existe.';
END
GO
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

-- ============================================
-- Índices para optimización de búsquedas
-- ============================================

-- Índice en PedidoDetalle.PedidoId (soporte FK y búsquedas por pedido)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PedidoDetalle_PedidoId' AND object_id = OBJECT_ID('dbo.PedidoDetalle'))
BEGIN
    CREATE INDEX IX_PedidoDetalle_PedidoId ON dbo.PedidoDetalle(PedidoId);
    PRINT 'Índice IX_PedidoDetalle_PedidoId creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El índice IX_PedidoDetalle_PedidoId ya existe.';
END
GO

-- Índice en PedidoDetalle.ProductoId (búsquedas por producto)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PedidoDetalle_ProductoId' AND object_id = OBJECT_ID('dbo.PedidoDetalle'))
BEGIN
    CREATE INDEX IX_PedidoDetalle_ProductoId ON dbo.PedidoDetalle(ProductoId);
    PRINT 'Índice IX_PedidoDetalle_ProductoId creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El índice IX_PedidoDetalle_ProductoId ya existe.';
END
GO

-- Índice en PedidoCabecera.ClienteId (búsquedas por cliente)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PedidoCabecera_ClienteId' AND object_id = OBJECT_ID('dbo.PedidoCabecera'))
BEGIN
    CREATE INDEX IX_PedidoCabecera_ClienteId ON dbo.PedidoCabecera(ClienteId);
    PRINT 'Índice IX_PedidoCabecera_ClienteId creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El índice IX_PedidoCabecera_ClienteId ya existe.';
END
GO

-- Índice en PedidoCabecera.Fecha (búsquedas por fecha)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PedidoCabecera_Fecha' AND object_id = OBJECT_ID('dbo.PedidoCabecera'))
BEGIN
    CREATE INDEX IX_PedidoCabecera_Fecha ON dbo.PedidoCabecera(Fecha DESC);
    PRINT 'Índice IX_PedidoCabecera_Fecha creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El índice IX_PedidoCabecera_Fecha ya existe.';
END
GO

-- Índice compuesto en PedidoCabecera (ClienteId, Fecha) para reportes por cliente y fecha
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PedidoCabecera_ClienteId_Fecha' AND object_id = OBJECT_ID('dbo.PedidoCabecera'))
BEGIN
    CREATE INDEX IX_PedidoCabecera_ClienteId_Fecha ON dbo.PedidoCabecera(ClienteId, Fecha DESC);
    PRINT 'Índice IX_PedidoCabecera_ClienteId_Fecha creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El índice IX_PedidoCabecera_ClienteId_Fecha ya existe.';
END
GO

-- Índice en LogAuditoria.Fecha descendente (consultas recientes primero)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LogAuditoria_Fecha' AND object_id = OBJECT_ID('dbo.LogAuditoria'))
BEGIN
    CREATE INDEX IX_LogAuditoria_Fecha ON dbo.LogAuditoria(Fecha DESC);
    PRINT 'Índice IX_LogAuditoria_Fecha creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El índice IX_LogAuditoria_Fecha ya existe.';
END
GO

-- Índice en LogAuditoria.Evento (búsquedas por tipo de evento)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LogAuditoria_Evento' AND object_id = OBJECT_ID('dbo.LogAuditoria'))
BEGIN
    CREATE INDEX IX_LogAuditoria_Evento ON dbo.LogAuditoria(Evento);
    PRINT 'Índice IX_LogAuditoria_Evento creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El índice IX_LogAuditoria_Evento ya existe.';
END
GO

-- Índice compuesto en LogAuditoria (Evento, Fecha) para filtros combinados
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LogAuditoria_Evento_Fecha' AND object_id = OBJECT_ID('dbo.LogAuditoria'))
BEGIN
    CREATE INDEX IX_LogAuditoria_Evento_Fecha ON dbo.LogAuditoria(Evento, Fecha DESC);
    PRINT 'Índice IX_LogAuditoria_Evento_Fecha creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El índice IX_LogAuditoria_Evento_Fecha ya existe.';
END
GO
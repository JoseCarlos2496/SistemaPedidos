-- 1. Crear un login
USE master;
GO

CREATE LOGIN pedidos_user WITH PASSWORD = 'Pedidos2024!';
GO

-- 2. Crear usuario en la base de datos
USE SistemaPedido;
GO

CREATE USER pedidos_user FOR LOGIN pedidos_user;
GO

-- 3. Asignar permisos
ALTER ROLE db_owner ADD MEMBER pedidos_user;
GO
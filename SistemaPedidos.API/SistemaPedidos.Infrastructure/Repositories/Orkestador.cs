using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Data;

namespace SistemaPedidos.Infrastructure.Repositories
{
    /// <summary>
    /// Implementa patrón Unit of Work coordinando repositorios y transacciones.
    /// Proporciona acceso unificado a repositorios y gestión transaccional.
    /// </summary>
    /// <remarks>
    /// Responsabilidades:
    /// - Proporcionar instancias únicas de repositorios (lazy loading)
    /// - Coordinar SaveChanges() entre múltiples repositorios
    /// - Gestionar transacciones de base de datos (Begin/Commit/Rollback)
    /// - Ejecutar operaciones dentro de estrategia de reintentos de EF Core
    /// 
    /// Patrón implementado: Unit of Work + Repository Pattern.
    /// </remarks>
    public class Orkestador : IOrkestador
    {
        private readonly SistemaPedidosDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        private IPedidoRepository? _pedidoRepository;
        private ILogAuditoriaRepository? _logAuditoriaRepository;

        /// <summary>
        /// Constructor que recibe el DbContext.
        /// </summary>
        public Orkestador(SistemaPedidosDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Repositorio de pedidos (lazy loading).
        /// Crea instancia solo cuando se accede por primera vez.
        /// </summary>
        public IPedidoRepository Pedidos
        {
            get
            {
                _pedidoRepository ??= new PedidoRepository(_context);
                return _pedidoRepository;
            }
        }

        /// <summary>
        /// Repositorio de auditoría (lazy loading).
        /// Crea instancia solo cuando se accede por primera vez.
        /// </summary>
        public ILogAuditoriaRepository LogAuditoria
        {
            get
            {
                _logAuditoriaRepository ??= new LogAuditoriaRepository(_context);
                return _logAuditoriaRepository;
            }
        }

        /// <summary>
        /// Persiste todos los cambios pendientes en el contexto.
        /// Ejecuta INSERT/UPDATE/DELETE en base de datos.
        /// </summary>
        /// <returns>Número de registros afectados</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Inicia una transacción de base de datos.
        /// </summary>
        /// <remarks>
        /// Todas las operaciones posteriores se ejecutan dentro de la transacción.
        /// Ejecutado dentro de ExecuteInStrategyAsync para soporte de reintentos.
        /// Lanza InvalidOperationException si ya existe transacción activa.
        /// </remarks>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Ya existe una transacción activa");
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            });
        }

        /// <summary>
        /// Confirma la transacción actual persistiendo todos los cambios.
        /// </summary>
        /// <remarks>
        /// Ejecuta SaveChanges() antes del commit.
        /// En caso de error ejecuta rollback automático.
        /// Libera la transacción al finalizar.
        /// </remarks>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);

                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        /// <summary>
        /// Revierte la transacción actual descartando todos los cambios.
        /// </summary>
        /// <remarks>
        /// Ejecuta ROLLBACK en base de datos.
        /// Libera la transacción al finalizar.
        /// </remarks>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Ejecuta operación dentro de estrategia de reintentos de EF Core.
        /// </summary>
        /// <remarks>
        /// Reintenta automáticamente en errores transitorios:
        /// - Timeout de conexión
        /// - Deadlock
        /// - Pérdida de conexión transitoria
        /// 
        /// Configurado en Program.cs con EnableRetryOnFailure (max 3 reintentos, 5 seg delay).
        /// </remarks>
        /// <typeparam name="T">Tipo de retorno de la operación</typeparam>
        /// <param name="operation">Operación asíncrona a ejecutar</param>
        public async Task<T> ExecuteInStrategyAsync<T>(Func<Task<T>> operation)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(operation);
        }

        /// <summary>
        /// Ejecuta operación sin retorno dentro de estrategia de reintentos.
        /// </summary>
        public async Task ExecuteInStrategyAsync(Func<Task> operation)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(operation);
        }

        /// <summary>
        /// Libera recursos del contexto y transacción.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implementación del patrón Dispose.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
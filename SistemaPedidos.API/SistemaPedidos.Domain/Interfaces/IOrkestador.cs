namespace SistemaPedidos.Domain.Interfaces
{
    /// <summary>
    /// Contrato para el orquestador que implementa patrón Unit of Work.
    /// Coordina repositorios, transacciones y estrategia de reintentos.
    /// </summary>
    /// <remarks>
    /// Responsabilidades:
    /// - Proporcionar acceso unificado a repositorios
    /// - Gestionar transacciones de base de datos
    /// - Coordinar SaveChanges() entre múltiples repositorios
    /// - Ejecutar operaciones dentro de estrategia de reintentos
    /// 
    /// Implementado por clase Orkestador en capa Infrastructure.
    /// </remarks>
    public interface IOrkestador : IDisposable
    {
        /// <summary>
        /// Repositorio de pedidos (PedidoCabecera y PedidoDetalle).
        /// </summary>
        IPedidoRepository Pedidos { get; }

        /// <summary>
        /// Repositorio de auditoría (LogAuditoria).
        /// </summary>
        ILogAuditoriaRepository LogAuditoria { get; }

        /// <summary>
        /// Persiste todos los cambios pendientes en el contexto de Entity Framework.
        /// Ejecuta INSERT/UPDATE/DELETE en base de datos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de registros afectados</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Inicia una transacción de base de datos.
        /// Todas las operaciones posteriores se ejecutan dentro de la transacción.
        /// Requiere CommitTransactionAsync() o RollbackTransactionAsync().
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirma (commit) la transacción actual.
        /// Persiste todos los cambios en base de datos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Revierte (rollback) la transacción actual.
        /// Descarta todos los cambios pendientes.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta una operación dentro de la estrategia de reintentos de EF Core.
        /// Reintenta automáticamente en errores transitorios (timeout, deadlock).
        /// </summary>
        /// <typeparam name="T">Tipo de retorno de la operación</typeparam>
        /// <param name="operation">Operación asíncrona a ejecutar</param>
        /// <returns>Resultado de la operación</returns>
        Task<T> ExecuteInStrategyAsync<T>(Func<Task<T>> operation);

        /// <summary>
        /// Ejecuta una operación sin retorno dentro de la estrategia de reintentos.
        /// </summary>
        /// <param name="operation">Operación asíncrona a ejecutar</param>
        Task ExecuteInStrategyAsync(Func<Task> operation);
    }
}
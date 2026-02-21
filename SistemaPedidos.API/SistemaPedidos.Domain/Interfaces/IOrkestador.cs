namespace SistemaPedidos.Domain.Interfaces
{
    public interface IOrkestador : IDisposable
    {
        IPedidoRepository Pedidos { get; }
        ILogAuditoriaRepository LogAuditoria { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
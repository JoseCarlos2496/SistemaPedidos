using Microsoft.EntityFrameworkCore.Storage;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Data;

namespace SistemaPedidos.Infrastructure.Repositories
{
    public class Orkestador : IOrkestador
    {
        private readonly SistemaPedidosDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        private IPedidoRepository? _pedidoRepository;
        private ILogAuditoriaRepository? _logAuditoriaRepository;

        public Orkestador(SistemaPedidosDbContext context)
        {
            _context = context;
        }

        public IPedidoRepository Pedidos
        {
            get
            {
                _pedidoRepository ??= new PedidoRepository(_context);
                return _pedidoRepository;
            }
        }

        public ILogAuditoriaRepository LogAuditoria
        {
            get
            {
                _logAuditoriaRepository ??= new LogAuditoriaRepository(_context);
                return _logAuditoriaRepository;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

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

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
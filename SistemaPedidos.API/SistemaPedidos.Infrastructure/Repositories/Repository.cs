using Microsoft.EntityFrameworkCore;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Data;
using System.Linq.Expressions;

namespace SistemaPedidos.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación genérica del patrón Repository usando Entity Framework Core.
    /// Proporciona operaciones CRUD estándar para cualquier entidad.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad del dominio</typeparam>
    /// <remarks>
    /// Clase base para repositorios específicos (PedidoRepository, LogAuditoriaRepository).
    /// Usa DbContext de EF Core para acceso a datos.
    /// NO ejecuta SaveChanges(), esa responsabilidad es del Unit of Work (Orkestador).
    /// </remarks>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SistemaPedidosDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Constructor que inicializa el repositorio con el contexto de EF Core.
        /// </summary>
        /// <param name="context">DbContext de la aplicación</param>
        public Repository(SistemaPedidosDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Obtiene una entidad por ID usando FindAsync de EF Core.
        /// </summary>
        /// <remarks>
        /// FindAsync busca primero en el contexto local (cache de EF Core) antes de ir a BD.
        /// Retorna null si no encuentra la entidad.
        /// </remarks>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Obtiene todas las entidades ejecutando ToListAsync.
        /// </summary>
        /// <remarks>
        /// ADVERTENCIA: Carga todas las filas en memoria. Peligroso en tablas grandes.
        /// </remarks>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Busca entidades que cumplan el predicado usando Where + ToListAsync.
        /// </summary>
        /// <remarks>
        /// El predicado se traduce a SQL por EF Core (expression tree).
        /// </remarks>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Agrega entidad al contexto con estado Added.
        /// </summary>
        /// <remarks>
        /// Retorna la entidad con estado Added.
        /// El ID se genera al ejecutar SaveChanges().
        /// </remarks>
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        /// <summary>
        /// Marca entidad como modificada (estado Modified).
        /// </summary>
        /// <remarks>
        /// EF Core rastreará cambios en propiedades y generará UPDATE.
        /// </remarks>
        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        /// <summary>
        /// Marca entidad para eliminación (estado Deleted).
        /// </summary>
        /// <remarks>
        /// Ejecuta DELETE al hacer SaveChanges().
        /// </remarks>
        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
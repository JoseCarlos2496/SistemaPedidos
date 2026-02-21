using System.Linq.Expressions;

namespace SistemaPedidos.Domain.Interfaces
{
    /// <summary>
    /// Contrato genérico para repositorios con operaciones CRUD estándar.
    /// Implementa patrón Repository para abstraer acceso a datos.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad del dominio (clase)</typeparam>
    /// <remarks>
    /// Proporciona métodos comunes de lectura y escritura para cualquier entidad.
    /// Implementado por Repository&lt;T&gt; en capa Infrastructure usando Entity Framework Core.
    /// 
    /// Interfaces específicas (IPedidoRepository, ILogAuditoriaRepository) heredan de esta
    /// y pueden agregar métodos especializados según necesidades del dominio.
    /// 
    /// Patrón utilizado: Repository Pattern + Unit of Work (vía IOrkestador).
    /// </remarks>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Obtiene una entidad por su identificador único.
        /// </summary>
        /// <param name="id">ID de la entidad (clave primaria)</param>
        /// <returns>Entidad encontrada o null si no existe</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las entidades del tipo especificado.
        /// </summary>
        /// <returns>Colección de todas las entidades en la tabla</returns>
        /// <remarks>
        /// ADVERTENCIA: Usar con precaución en tablas grandes.
        /// Considerar paginación o filtros para datasets extensos.
        /// </remarks>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Busca entidades que cumplan con un predicado específico.
        /// </summary>
        /// <param name="predicate">Expresión lambda para filtrar (ej: x => x.ClienteId == 1)</param>
        /// <returns>Colección de entidades que cumplen el predicado</returns>
        /// <remarks>
        /// Permite queries tipo LINQ sobre la entidad.
        /// Ejemplo: FindAsync(p => p.ClienteId == 1 &amp;&amp; p.Total > 1000)
        /// </remarks>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Agrega una nueva entidad al contexto de Entity Framework.
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <returns>Entidad agregada al contexto (con estado Added)</returns>
        /// <remarks>
        /// NO persiste inmediatamente. Requiere SaveChanges() del Unit of Work.
        /// El ID se genera al ejecutar SaveChanges() si es IDENTITY.
        /// </remarks>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Marca una entidad como modificada en el contexto de Entity Framework.
        /// </summary>
        /// <param name="entity">Entidad con cambios a persistir</param>
        /// <remarks>
        /// NO persiste inmediatamente. Requiere SaveChanges().
        /// EF Core detecta automáticamente propiedades modificadas.
        /// Método síncrono porque solo cambia estado en el contexto.
        /// </remarks>
        void Update(T entity);

        /// <summary>
        /// Marca una entidad para eliminación del contexto de Entity Framework.
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <remarks>
        /// NO elimina inmediatamente. Requiere SaveChanges().
        /// Ejecuta DELETE en base de datos al hacer SaveChanges().
        /// Método síncrono porque solo cambia estado en el contexto.
        /// </remarks>
        void Remove(T entity);
    }
}
using System.Linq.Expressions;

namespace RentaFacil.Shared.Interfaces;

public interface IRepository<T> where T : class
{
    // Basic CRUD operations
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    
    // Add and Update operations
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    
    // Delete operations
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    
    // Persistence
    Task<int> SaveChangesAsync();
    
    // Query operations with pagination
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
}
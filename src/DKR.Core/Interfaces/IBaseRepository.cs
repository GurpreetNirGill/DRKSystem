using System.Linq.Expressions;

namespace DKR.Core.Interfaces;

public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<int> CountAsync();
    Task<bool> ExistsAsync(string id);
}
using System.Linq.Expressions;

namespace FinanceHub.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> FindAllAsync(params Expression<Func<T, object>>[] includes);
        Task<T?> FindByIdAsync(int id, params Expression<Func<T, object>>[] includes);
        Task InsertAsync(T entity);
        Task UpdateAsync(T entity);
        Task RemoveAsync(T entity);
        Task<bool> ExistsAsync(int id);
    }
}

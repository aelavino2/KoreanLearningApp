using System.Linq.Expressions;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions
{
    public interface IRepository<T> where T : new()
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<int> InsertAsync(T entity);
        Task<int> InsertAllAsync(List<T> entities);
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(T entity);
        Task<int> DeleteWhereAsync(Expression<Func<T, bool>> predicate);
    }
}

using System.Linq.Expressions;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class BaseRepository<T>(DbContext dbContext) : IRepository<T>
    where T : new()
{
    public async Task<List<T>> GetAllAsync()
    {
        var db = await dbContext.GetConnectionAsync();
        return await db.Table<T>().ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        var db = await dbContext.GetConnectionAsync();
        return await db.FindAsync<T>(id);
    }

    public async Task<int> InsertAsync(T entity)
    {
        var db = await dbContext.GetConnectionAsync();
        return await db.InsertAsync(entity);
    }

    public async Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate)
    {
        var db = await dbContext.GetConnectionAsync();
        return await db.Table<T>().Where(predicate).ToListAsync();
    }
    
    public async Task<int> InsertAllAsync(List<T> entities)
    {
        if (entities.Count == 0)
            return 0;

        var db = await dbContext.GetConnectionAsync();
        return await db.InsertAllAsync(entities);
    }

    public async Task<int> UpdateAsync(T entity)
    {
        var db = await dbContext.GetConnectionAsync();
        return await db.UpdateAsync(entity);
    }

    public async Task<int> DeleteAsync(T entity)
    {
        var db = await dbContext.GetConnectionAsync();
        return await db.DeleteAsync(entity);
    }

    public async Task<int> DeleteWhereAsync(Expression<Func<T, bool>> predicate)
    {
        var db = await dbContext.GetConnectionAsync();
        return await db.Table<T>().DeleteAsync(predicate);
    }
}
using KoreanLearningApp.Infrastructure.Persistence;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using System.Linq.Expressions;

public class BaseRepository<T> : IRepository<T> where T : new()
{
    private readonly DbContext _dbContext;

    public BaseRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<T>> GetAllAsync()
    {
        var db = await _dbContext.GetConnectionAsync();
        return await db.Table<T>().ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        var db = await _dbContext.GetConnectionAsync();
        return await db.FindAsync<T>(id);
    }

    public async Task<int> InsertAsync(T entity)
    {
        var db = await _dbContext.GetConnectionAsync();
        return await db.InsertAsync(entity);
    }

    public async Task<int> InsertAllAsync(List<T> entities)
    {
        if (entities.Count == 0)
            return 0;

        var db = await _dbContext.GetConnectionAsync();
        return await db.InsertAllAsync(entities);
    }

    public async Task<int> UpdateAsync(T entity)
    {
        var db = await _dbContext.GetConnectionAsync();
        return await db.UpdateAsync(entity);
    }

    public async Task<int> DeleteAsync(T entity)
    {
        var db = await _dbContext.GetConnectionAsync();
        return await db.DeleteAsync(entity);
    }

    public async Task<int> DeleteWhereAsync(Expression<Func<T, bool>> predicate)
    {
        var db = await _dbContext.GetConnectionAsync();
        return await db.Table<T>().DeleteAsync(predicate);
    }
}
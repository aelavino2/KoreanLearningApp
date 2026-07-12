using SQLite;
using KoreanLearningApp.Infrastructure.Persistence.Entities;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class Repository<TEntity> where TEntity : class, IEntity, new()
{
    protected readonly DbContext DbContext;

    public Repository(DbContext dbContext)
    {
        DbContext = dbContext;
    }
    
    public async Task<List<TEntity>> GetAllEntitiesAsync()
    {
        var db = await DbContext.GetConnectionAsync();
        return await db.Table<TEntity>().ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        var db = await DbContext.GetConnectionAsync();
        return await db.Table<TEntity>().FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<int> SaveAsync(TEntity entity)
    {
        var db = await DbContext.GetConnectionAsync();

        return entity.Id != 0
            ? await db.UpdateAsync(entity)
            : await db.InsertAsync(entity);
    }

    public async Task<int> InsertAllAsync(IEnumerable<TEntity> entities)
    {
        var db = await DbContext.GetConnectionAsync();
        return await db.InsertAllAsync(entities);
    }

    public async Task<int> DeleteAsync(TEntity entity)
    {
        var db = await DbContext.GetConnectionAsync();
        return await db.DeleteAsync(entity);
    }

    public async Task<int> CountAsync()
    {
        var db = await DbContext.GetConnectionAsync();
        return await db.Table<TEntity>().CountAsync();
    }
}
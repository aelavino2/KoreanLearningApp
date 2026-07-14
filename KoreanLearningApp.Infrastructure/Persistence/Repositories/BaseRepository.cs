using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories
{
    public class BaseRepository<T> : IRepository<T> where T : new()
    {
        protected SQLiteAsyncConnection Database { get; }

        public BaseRepository(SQLiteAsyncConnection database)
        {
            Database = database;
        }

        public async Task<List<T>> GetAllAsync() => await Database.Table<T>().ToListAsync();
        public async Task<T> GetByIdAsync(int id) => await Database.FindAsync<T>(id);
        public async Task<int> InsertAsync(T entity) => await Database.InsertAsync(entity);
        public async Task<int> UpdateAsync(T entity) => await Database.UpdateAsync(entity);
        public async Task<int> DeleteAsync(T entity) => await Database.DeleteAsync(entity);
    }
}

using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Models;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public abstract class ImportExportRepository<TEntity, TDto> : Repository<TEntity>
    where TEntity : class, IEntity, new()
{
    protected ImportExportRepository(DbContext dbContext) : base(dbContext)
    {
    }

    protected abstract bool IsValid(TDto dto);
    protected abstract string GetKey(TDto dto);
    protected abstract string GetKey(TEntity entity);
    protected abstract TEntity MapToEntity(TDto dto);

    protected abstract string InvalidMessage(TDto dto);
    protected abstract string DuplicateInBatchMessage(TDto dto);
    protected abstract string AlreadyExistsMessage(TDto dto);

    public async Task<ImportResult> ImportAsync(List<TDto> incoming)
    {
        var existing = await GetAllEntitiesAsync();

        var toInsert = new List<TEntity>();
        var skipped = new List<string>();
        var seenInBatch = new HashSet<string>();

        foreach (var dto in incoming)
        {
            if (!IsValid(dto))
            {
                skipped.Add(InvalidMessage(dto));
                continue;
            }

            var key = GetKey(dto);

            if (!seenInBatch.Add(key))
            {
                skipped.Add(DuplicateInBatchMessage(dto));
                continue;
            }

            if (existing.Any(e => GetKey(e) == key))
            {
                skipped.Add(AlreadyExistsMessage(dto));
                continue;
            }

            toInsert.Add(MapToEntity(dto));
        }

        if (toInsert.Count > 0)
            await InsertAllAsync(toInsert);

        return new ImportResult(toInsert.Count, skipped);
    }
}
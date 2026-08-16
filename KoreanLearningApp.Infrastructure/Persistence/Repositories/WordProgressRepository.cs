using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordProgressRepository(DbContext dbContext, IRepository<WordProgressEntity> progressRepo)
    : IWordProgressRepository
{
    public async Task<WordProgress?> GetByWordIdAsync(int wordId)
    {
        var entity = await GetEntityByWordIdAsync(wordId);
        return entity?.ToDomain();
    }

    public async Task<List<WordProgress>> GetDueAsync(DateTime asOf, int limit)
    {
        var db = await dbContext.GetConnectionAsync();

        const string sql = @"
            SELECT *
            FROM WordProgresses
            WHERE NextReviewDate <= ?
            ORDER BY NextReviewDate
            LIMIT ?";

        var rows = await db.QueryAsync<WordProgressEntity>(sql, asOf, limit);
        return rows.Select(r => r.ToDomain()).ToList();
    }

    public async Task<List<int>> GetNewWordIdsAsync(int limit)
    {
        var db = await dbContext.GetConnectionAsync();

        // Слова, для которых ещё нет ни одной записи прогресса, по частотности (Rank)
        const string sql = @"
            SELECT w.Id
            FROM Words w
            LEFT JOIN WordProgresses p ON p.WordId = w.Id
            WHERE p.Id IS NULL
            ORDER BY w.Rank
            LIMIT ?";

        // Мапим в Domain-модель Word — тот же приём, что и в GetWordsPageQuery:
        // из всех колонок Word нам нужна только Id, остальные останутся дефолтными.
        var rows = await db.QueryAsync<Word>(sql, limit);
        return rows.Select(r => r.Id).ToList();
    }

    public async Task UpsertAsync(WordProgress progress)
    {
        var existing = await GetEntityByWordIdAsync(progress.WordId);
        var entity = progress.ToEntity();

        if (existing is null)
        {
            entity.Id = 0;
            await progressRepo.InsertAsync(entity);
        }
        else
        {
            entity.Id = existing.Id;
            await progressRepo.UpdateAsync(entity);
        }

        progress.Id = entity.Id;
    }

    private async Task<WordProgressEntity?> GetEntityByWordIdAsync(int wordId)
    {
        var results = await progressRepo.GetWhereAsync(p => p.WordId == wordId);
        return results.FirstOrDefault();
    }
}
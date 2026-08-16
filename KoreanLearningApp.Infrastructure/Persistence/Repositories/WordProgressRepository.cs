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

    public async Task<List<WordProgress>> GetByWordIdsAsync(List<int> wordIds)
    {
        if (wordIds.Count == 0)
            return new List<WordProgress>();

        var entities = await progressRepo.GetWhereAsync(p => wordIds.Contains(p.WordId));
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<List<WordProgress>> GetDueAsync(DateTime asOf, int limit, IReadOnlyCollection<string>? topikLevels = null)
    {
        var db = await dbContext.GetConnectionAsync();
        var levels = NormalizeLevels(topikLevels);

        if (levels.Count == 0)
        {
            const string sql = @"
                SELECT *
                FROM WordProgresses
                WHERE NextReviewDate <= ?
                ORDER BY NextReviewDate
                LIMIT ?";

            var rows = await db.QueryAsync<WordProgressEntity>(sql, asOf, limit);
            return rows.Select(r => r.ToDomain()).ToList();
        }

        // JOIN с Words нужен только когда задан фильтр по уровню — без него достаточно
        // и исходного плоского запроса по WordProgresses.
        var placeholders = string.Join(",", levels.Select(_ => "?"));
        var filteredSql = $@"
            SELECT p.*
            FROM WordProgresses p
            JOIN Words w ON w.Id = p.WordId
            WHERE p.NextReviewDate <= ? AND w.TopikLevel IN ({placeholders})
            ORDER BY p.NextReviewDate
            LIMIT ?";

        var args = new List<object> { asOf };
        args.AddRange(levels);
        args.Add(limit);

        var filteredRows = await db.QueryAsync<WordProgressEntity>(filteredSql, args.ToArray());
        return filteredRows.Select(r => r.ToDomain()).ToList();
    }

    public async Task<List<int>> GetNewWordIdsAsync(int limit, IReadOnlyCollection<string>? topikLevels = null)
    {
        var db = await dbContext.GetConnectionAsync();
        var levels = NormalizeLevels(topikLevels);

        if (levels.Count == 0)
        {
            const string sql = @"
                SELECT w.Id
                FROM Words w
                LEFT JOIN WordProgresses p ON p.WordId = w.Id
                WHERE p.Id IS NULL
                ORDER BY w.Rank
                LIMIT ?";

            var rows = await db.QueryAsync<Word>(sql, limit);
            return rows.Select(r => r.Id).ToList();
        }

        var placeholders = string.Join(",", levels.Select(_ => "?"));
        var filteredSql = $@"
            SELECT w.Id
            FROM Words w
            LEFT JOIN WordProgresses p ON p.WordId = w.Id
            WHERE p.Id IS NULL AND w.TopikLevel IN ({placeholders})
            ORDER BY w.Rank
            LIMIT ?";

        var args = new List<object>();
        args.AddRange(levels);
        args.Add(limit);

        var filteredRows = await db.QueryAsync<Word>(filteredSql, args.ToArray());
        return filteredRows.Select(r => r.Id).ToList();
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

    private static List<string> NormalizeLevels(IReadOnlyCollection<string>? topikLevels) =>
        (topikLevels ?? Array.Empty<string>())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .Distinct()
            .ToList();

    private async Task<WordProgressEntity?> GetEntityByWordIdAsync(int wordId)
    {
        var results = await progressRepo.GetWhereAsync(p => p.WordId == wordId);
        return results.FirstOrDefault();
    }
}
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public sealed class WordProgressRepository(DbContext dbContext, IRepository<WordProgressEntity> progressRepo)
    : IWordProgressRepository
{
    public async Task<WordProgress?> GetByWordIdAsync(int wordId) =>
        (await GetEntityByWordIdAsync(wordId))?.ToDomain();

    public async Task<List<WordProgress>> GetByWordIdsAsync(IReadOnlyCollection<int> wordIds)
    {
        if (wordIds.Count == 0)
            return [];

        var entities = await progressRepo.GetWhereAsync(p => wordIds.Contains(p.WordId));
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<List<WordProgress>> GetDueAsync(DateTime asOf, int limit, IReadOnlyCollection<string> topikLevels)
    {
        var db = await dbContext.GetConnectionAsync();
        var (levelFilter, levelArgs) = BuildTopikLevelFilter(topikLevels);

        var sql = $"""
            SELECT p.*
            FROM WordProgresses p
            JOIN Words w ON w.Id = p.WordId
            WHERE p.NextReviewDate <= ? {levelFilter}
            ORDER BY p.NextReviewDate
            LIMIT ?
            """;

        object[] args = [asOf, .. levelArgs, limit];
        var rows = await db.QueryAsync<WordProgressEntity>(sql, args);
        return rows.Select(r => r.ToDomain()).ToList();
    }

    public async Task<List<int>> GetNewWordIdsAsync(int limit, IReadOnlyCollection<string> topikLevels)
    {
        var db = await dbContext.GetConnectionAsync();
        var (levelFilter, levelArgs) = BuildTopikLevelFilter(topikLevels);

        var sql = $"""
            SELECT w.Id
            FROM Words w
            LEFT JOIN WordProgresses p ON p.WordId = w.Id
            WHERE p.Id IS NULL {levelFilter}
            ORDER BY w.Rank
            LIMIT ?
            """;

        object[] args = [.. levelArgs, limit];
        var rows = await db.QueryAsync<WordEntity>(sql, args);
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

    private static (string Sql, object[] Args) BuildTopikLevelFilter(IReadOnlyCollection<string> topikLevels)
    {
        if (topikLevels.Count == 0)
            return (string.Empty, []);

        var placeholders = string.Join(",", topikLevels.Select(_ => "?"));
        return ($"AND w.TopikLevel IN ({placeholders})", topikLevels.Cast<object>().ToArray());
    }

    private async Task<WordProgressEntity?> GetEntityByWordIdAsync(int wordId) =>
        (await progressRepo.GetWhereAsync(p => p.WordId == wordId)).FirstOrDefault();
}

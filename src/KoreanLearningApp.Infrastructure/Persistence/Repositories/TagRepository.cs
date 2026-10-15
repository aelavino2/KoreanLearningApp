using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public sealed class TagRepository(DbContext dbContext) : ITagRepository
{
    public async Task<List<TagUsage>> GetAllWithUsageAsync()
    {
        const string sql = """
            SELECT t.Id, t.NameKo, t.NameRu, COUNT(wt.Id) AS WordCount
            FROM Tags t
            LEFT JOIN WordTags wt ON wt.TagId = t.Id
            GROUP BY t.Id, t.NameKo, t.NameRu
            ORDER BY t.NameRu, t.NameKo
            """;

        var db = await dbContext.GetConnectionAsync();
        var rows = await db.QueryAsync<TagUsageRow>(sql);
        return rows
            .Select(r => new TagUsage(new Tag { Id = r.Id, NameKo = r.NameKo, NameRu = r.NameRu }, r.WordCount))
            .ToList();
    }

    private sealed class TagUsageRow
    {
        public int Id { get; set; }
        public string NameKo { get; set; } = string.Empty;
        public string NameRu { get; set; } = string.Empty;
        public int WordCount { get; set; }
    }
}

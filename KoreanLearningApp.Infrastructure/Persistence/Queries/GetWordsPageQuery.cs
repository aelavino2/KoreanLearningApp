using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Queries;

public class GetWordsPageQuery(DbContext dbContext) : IGetWordsPageQuery
{
    public async Task<(List<int> Ids, int TotalCount)> ExecuteAsync(int page, int pageSize, string? search)
    {
        var db = await dbContext.GetConnectionAsync();

        var hasSearch = !string.IsNullOrWhiteSpace(search);
        var likePattern = hasSearch ? $"%{search}%" : null;

        var totalCount = await GetTotalCountAsync(db, hasSearch, likePattern);
        var pageIds = await GetPageIdsAsync(db, hasSearch, likePattern, page, pageSize);

        return (pageIds, totalCount);
    }

    private static async Task<int> GetTotalCountAsync(SQLiteAsyncConnection db, bool hasSearch, string? likePattern)
    {
        if (!hasSearch)
            return await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Words");

        const string countSql = @"
            SELECT COUNT(DISTINCT w.Id)
            FROM Words w
            LEFT JOIN KrDicts k ON k.WordId = w.Id
            LEFT JOIN KrDictSenses s ON s.KrDictId = k.Id
            LEFT JOIN LangInfos li ON li.Id = s.RuId
            WHERE w.Korean LIKE ? OR li.Word LIKE ?";

        return await db.ExecuteScalarAsync<int>(countSql, likePattern, likePattern);
    }

    private static async Task<List<int>> GetPageIdsAsync(
        SQLiteAsyncConnection db, bool hasSearch, string? likePattern, int page, int pageSize)
    {
        var offset = (page - 1) * pageSize;

        if (!hasSearch)
        {
            const string plainSql = "SELECT Id FROM Words ORDER BY Rank LIMIT ? OFFSET ?";
            var plainRows = await db.QueryAsync<IdRow>(plainSql, pageSize, offset);
            return plainRows.Select(r => r.Id).ToList();
        }

        const string searchSql = @"
            SELECT DISTINCT w.Id
            FROM Words w
            LEFT JOIN KrDicts k ON k.WordId = w.Id
            LEFT JOIN KrDictSenses s ON s.KrDictId = k.Id
            LEFT JOIN LangInfos li ON li.Id = s.RuId
            WHERE w.Korean LIKE ? OR li.Word LIKE ?
            ORDER BY w.Rank
            LIMIT ? OFFSET ?";

        var searchRows = await db.QueryAsync<IdRow>(searchSql, likePattern, likePattern, pageSize, offset);
        return searchRows.Select(r => r.Id).ToList();
    }

    private class IdRow
    {
        public int Id { get; set; }
    }
}
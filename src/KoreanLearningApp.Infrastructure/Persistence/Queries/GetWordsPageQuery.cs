using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Infrastructure.Persistence.Entities;

namespace KoreanLearningApp.Infrastructure.Persistence.Queries;

public class GetWordsPageQuery(DbContext dbContext) : IGetWordsPageQuery
{
    private const string SearchCondition = """
        (w.Korean LIKE ? OR EXISTS (
            SELECT 1
            FROM KrDicts k
            JOIN KrDictSenses s ON s.KrDictId = k.Id
            LEFT JOIN LangInfos liRu ON liRu.Id = s.RuId
            LEFT JOIN LangInfos liEn ON liEn.Id = s.EnId
            WHERE k.WordId = w.Id AND (liRu.Word LIKE ? OR liEn.Word LIKE ?)))
        """;

    private const string FirstTagName = """
        (SELECT MIN(t.NameRu) FROM WordTags wt JOIN Tags t ON t.Id = wt.TagId WHERE wt.WordId = w.Id)
        """;

    public async Task<(List<int> Ids, int TotalCount)> ExecuteAsync(WordsPageRequest request)
    {
        var db = await dbContext.GetConnectionAsync();
        var (where, args) = BuildFilter(request);

        var totalCount = await db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Words w {where}", args.ToArray());

        var pageSql = $"SELECT w.Id FROM Words w {where} ORDER BY {BuildOrder(request.Sort)} LIMIT ? OFFSET ?";
        object[] pageArgs = [.. args, request.PageSize, (request.Page - 1) * request.PageSize];
        var rows = await db.QueryAsync<WordEntity>(pageSql, pageArgs);

        return (rows.Select(r => r.Id).ToList(), totalCount);
    }

    private static (string Sql, List<object> Args) BuildFilter(WordsPageRequest request)
    {
        var conditions = new List<string>();
        var args = new List<object>();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var likePattern = $"%{request.Search.Trim()}%";
            conditions.Add(SearchCondition);
            args.AddRange([likePattern, likePattern, likePattern]);
        }

        if (request.TagIds is { Count: > 0 } tagIds)
        {
            var placeholders = string.Join(",", tagIds.Select(_ => "?"));
            conditions.Add($"EXISTS (SELECT 1 FROM WordTags wt WHERE wt.WordId = w.Id AND wt.TagId IN ({placeholders}))");
            args.AddRange(tagIds.Cast<object>());
        }

        var sql = conditions.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", conditions);
        return (sql, args);
    }

    private static string BuildOrder(WordSortEnum sort) => sort switch
    {
        WordSortEnum.Korean => "w.Korean, w.Rank, w.Id",
        WordSortEnum.Tag => $"{FirstTagName} IS NULL, {FirstTagName}, w.Rank, w.Id",
        _ => "w.Rank, w.Id"
    };
}

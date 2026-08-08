namespace KoreanLearningApp.Infrastructure.Persistence.Queries;

public interface IGetWordsPageQuery
{
    Task<(List<int> Ids, int TotalCount)> ExecuteAsync(int page, int pageSize, string? search);
}
using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Infrastructure.Persistence.Queries;

public interface IGetWordsPageQuery
{
    Task<(List<int> Ids, int TotalCount)> ExecuteAsync(WordsPageRequest request);
}

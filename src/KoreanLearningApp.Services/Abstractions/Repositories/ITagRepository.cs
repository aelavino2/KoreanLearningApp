using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface ITagRepository
{
    /// <summary>
    /// Все теги с числом слов, отсортированные по русскому названию.
    /// </summary>
    Task<List<TagUsage>> GetAllWithUsageAsync();
}

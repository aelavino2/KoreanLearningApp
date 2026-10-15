namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface ISavedWordRepository
{
    /// <summary>
    /// Id сохранённых слов, последние сохранённые — первыми.
    /// </summary>
    Task<List<int>> GetSavedWordIdsAsync();

    Task<bool> IsSavedAsync(int wordId);
    Task AddAsync(int wordId);
    Task RemoveAsync(int wordId);
}

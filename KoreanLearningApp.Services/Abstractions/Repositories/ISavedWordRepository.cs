namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface ISavedWordRepository
{
    Task<List<int>> GetSavedWordIdsAsync();
    Task<bool> IsSavedAsync(int wordId);
    Task AddAsync(int wordId);
    Task RemoveAsync(int wordId);
}
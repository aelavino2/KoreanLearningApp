using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Infrastructure.Persistence.Abstractions;

public interface IBackupService
{
    string BuildExportJson(IReadOnlyList<Word> words);
    Task SaveAsync(IReadOnlyList<Word> words);
}
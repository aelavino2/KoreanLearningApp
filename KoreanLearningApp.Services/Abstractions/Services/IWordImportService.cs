using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IWordImportService
{
    Task<int> ImportFromFileAsync(string filePath);
    Task<int> ImportFromJsonAsync(string json);
    Task<string> ExportToJsonAsync(IEnumerable<Word> words);
}
namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IWordImportService
{
    Task<int> ImportFromJsonAsync(string json);
    Task<int> ImportFromFileAsync(string filePath);
}
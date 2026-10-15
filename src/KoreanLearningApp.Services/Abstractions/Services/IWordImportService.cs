using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IWordImportService
{
    Task<WordImportResult> ImportFromFileAsync(string filePath);

    /// <summary>
    /// Добавляет новые слова; у слов, которые уже есть в словаре, дополняет теги.
    /// </summary>
    Task<WordImportResult> ImportFromJsonAsync(string json);
}
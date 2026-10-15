namespace KoreanLearningApp.Domain.Models;

/// <param name="AddedWords">Новые слова, которых не было в словаре.</param>
/// <param name="TaggedExistingWords">Уже существующие слова, которым добавлены новые теги.</param>
public record WordImportResult(int AddedWords, int TaggedExistingWords);

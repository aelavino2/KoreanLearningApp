namespace KoreanLearningApp.Presentation.Files;

public interface IFileService
{
    /// <summary>
    /// Открывает системный выбор JSON-файла и возвращает его содержимое; <c>null</c>, если пользователь отменил выбор.
    /// </summary>
    Task<string?> PickJsonFileAsync(string title);

    /// <summary>
    /// Сохраняет текст во временный файл и открывает системное окно «Поделиться».
    /// </summary>
    Task ShareTextFileAsync(string fileName, string content, string title);
}

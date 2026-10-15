using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Services
{
    public interface IWordExportService
    {
        Task<string> ExportToJsonAsync(IEnumerable<Word> words);
    }
}

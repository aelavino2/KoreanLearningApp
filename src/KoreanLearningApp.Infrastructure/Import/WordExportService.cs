using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Constants;
using KoreanLearningApp.Infrastructure.Import.DTO;
using KoreanLearningApp.Services.Abstractions.Services;
using Mapster;
using System.Text.Json;

namespace KoreanLearningApp.Infrastructure.Import;

public sealed class WordExportService(ExportJsonOptions options) : IWordExportService
{
    public Task<string> ExportToJsonAsync(IEnumerable<Word> words)
    {
        var dtos = words.Select(w => w.Adapt<WordJsonDto>());
        var json = JsonSerializer.Serialize(dtos, options.Value);
        return Task.FromResult(json);
    }
}
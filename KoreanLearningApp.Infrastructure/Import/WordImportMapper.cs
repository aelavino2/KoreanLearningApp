using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Import.DTO;
using Mapster;

namespace KoreanLearningApp.Infrastructure.Import;

internal static class WordImportMapper
{
    public static Word ToDomain(this WordJsonDto dto) => dto.Adapt<Word>();
    public static WordJsonDto ToDto(this Word word) => word.Adapt<WordJsonDto>();
}
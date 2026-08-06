using KoreanLearningApp.Infrastructure.Import.DTO;

namespace KoreanLearningApp.Infrastructure.Import.Abstraction
{
    public interface IKrDictJsonSerializer
    {
        List<WordJsonDto> DeserializeArray(string json);
        Task<List<WordJsonDto>> DeserializeFileAsync(string filePath);
    }
}

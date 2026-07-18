using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

public class WordEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Korean { get; set; } = string.Empty;
    public string TranslationRu { get; set; } = string.Empty;
    public string TranslationEn { get; set; } = string.Empty;
    public string RuleExplanation { get; set; } = string.Empty;
    public string PronunciationNote { get; set; } = string.Empty;

    public int Rank { get; set; }
    public string PartOfSpeech { get; set; } = string.Empty;
    public string Hanja { get; set; } = string.Empty;
    public string NiklLevel { get; set; } = string.Empty;
    public string TopikLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int SourceIndex { get; set; }

    public string TargetCode { get; set; } = string.Empty;
    public int SupNo { get; set; }
    public string Pos { get; set; } = string.Empty;
    public string WordGrade { get; set; } = string.Empty;
    public string DictLink { get; set; } = string.Empty;

    public string AudioUrl { get; set; } = string.Empty;
    public string AudioFile { get; set; } = string.Empty;
    public string SensesJson { get; set; } = "[]";

    [Indexed(Unique = true)]
    public string UniqueKey { get; set; } = string.Empty;
}
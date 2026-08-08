using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("Words")]
public class WordEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public string Korean { get; set; } = string.Empty;
    public string RuleExplanation { get; set; } = string.Empty;
    [Indexed]
    public int Rank { get; set; }
    public string PartOfSpeech { get; set; } = string.Empty;
    public string Hanja { get; set; } = string.Empty;
    public string NiklLevel { get; set; } = string.Empty;
    public string TopikLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int SourceIndex { get; set; }
}
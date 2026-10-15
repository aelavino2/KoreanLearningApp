using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Domain.Models;

public class Word
{
    public int Id { get; set; }
    public string Korean { get; set; } = string.Empty;
    public string RuleExplanation { get; set; } = string.Empty;

    public int Rank { get; set; }
    public PartOfSpeechEnum PartOfSpeech { get; set; }
    public string Hanja { get; set; } = string.Empty;
    public string NiklLevel { get; set; } = string.Empty;
    public string TopikLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int SourceIndex { get; set; }

    public KrDict? KrDict { get; set; }

    public List<Tag> Tags { get; set; } = [];
}
namespace KoreanLearningApp.Domain.Models;

public class Sense
{
    public int Id { get; set; }
    public string DefinitionKo { get; set; } = string.Empty;

    public int KrDictId { get; set; }
    public KrDict ParentKrDict { get; set; } = null!;

    public int? EnId { get; set; }
    public LangInfo? En { get; set; }

    public int? RuId { get; set; }
    public LangInfo? Ru { get; set; }
}
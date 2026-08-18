using KoreanLearningApp.Domain.Models;

public class KrDict
{
    public int Id { get; set; }
    public string TargetCode { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public int SupNo { get; set; }
    public string Pos { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
    public string WordGrade { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;

    public int WordId { get; set; }
    public Word ParentWord { get; set; } = null!;

    public List<Sense> Senses { get; set; } = new();

    public int? AudioId { get; set; }
    public Audio? Audio { get; set; }
}
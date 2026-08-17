namespace KoreanLearningApp.Domain.Models;

public class SavedWord
{
    public int Id { get; set; }
    public int WordId { get; set; }
    public DateTime SavedAt { get; set; }
}
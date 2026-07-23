using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("KrDicts")]
public class KrDictEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string TargetCode { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public int SupNo { get; set; }
    public string Pos { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
    public string WordGrade { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;

    [Indexed]
    public int WordId { get; set; }

    public int? AudioId { get; set; }
}
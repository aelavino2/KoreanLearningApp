using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table("KrDictSenses")]
public class KrDictSenseEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int KrDictId { get; set; }

    public string DefinitionKo { get; set; } = string.Empty;

    public int? EnId { get; set; }
    public int? RuId { get; set; }
}
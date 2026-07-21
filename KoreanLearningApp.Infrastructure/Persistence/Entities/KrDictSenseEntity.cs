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
    public string EnWord { get; set; } = string.Empty;
    public string EnDefinition { get; set; } = string.Empty;
    public string RuWord { get; set; } = string.Empty;
    public string RuDefinition { get; set; } = string.Empty;
}
using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

public interface IEntity
{
    [PrimaryKey, AutoIncrement]
    int Id { get; set; }
}

using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Constants;
using KoreanLearningApp.Models;
using Microsoft.VisualBasic;
using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Entities;

[Table(DbConstants.WordsTableName)]
internal class WordEntity : IEntity
{
    public string Korean { get; set; } = string.Empty;
    public string TranscriptionRu { get; set; } = string.Empty;
    public string TranscriptionEn { get; set; } = string.Empty;
    public string TranslationRu { get; set; } = string.Empty;
    public string TranslationEn { get; set; } = string.Empty;
    public string RuleExplanation { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public WordType Type { get; set; } = WordType.Other;
    public LearningStatus Status { get; set; } = LearningStatus.Learning;
    public string PronunciationNote { get; set; } = string.Empty;
    public int LeitnerBox { get; set; } = 1;
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewedAt { get; set; }
    public int Id { get; set; }
}
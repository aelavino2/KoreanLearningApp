using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Presentation.Formatting;

public static class TagDisplayExtensions
{
    /// <summary>
    /// «Дом · 집»; если русского названия нет — только корейское.
    /// </summary>
    public static string ToDisplayName(this Tag tag) =>
        string.IsNullOrWhiteSpace(tag.NameRu) ? tag.NameKo : $"{tag.NameRu} · {tag.NameKo}";
}

namespace KoreanLearningApp.Models;

public static class WordTypeHelper
{
    public static readonly (WordType Type, string Label)[] AllTypes = new[]
    {
        (WordType.Noun, "Существительное"),
        (WordType.Verb, "Глагол"),
        (WordType.Adjective, "Прилагательное"),
        (WordType.Adverb, "Наречие"),
        (WordType.Phrase, "Фраза"),
        (WordType.Number, "Числительное"),
        (WordType.Other, "Прочее"),
    };

    public static string ToLabel(WordType type)
        => AllTypes.FirstOrDefault(t => t.Type == type).Label ?? "Прочее";

    public static WordType FromLabel(string? label)
        => AllTypes.FirstOrDefault(t => t.Label.Equals(label, StringComparison.OrdinalIgnoreCase)).Type;

    public static WordType FromStringKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return WordType.Other;
        return Enum.TryParse<WordType>(key, ignoreCase: true, out var result) ? result : WordType.Other;
    }
}
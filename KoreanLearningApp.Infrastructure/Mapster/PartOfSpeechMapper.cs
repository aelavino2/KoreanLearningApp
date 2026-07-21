namespace KoreanLearningApp.Domain.Models.Enums;

public static class PartOfSpeechMapper
{
    public static PartOfSpeechEnum Parse(string? pos)
    {
        return pos?.Trim() switch
        {
            "명사" => PartOfSpeechEnum.Noun,
            "대명사" => PartOfSpeechEnum.Pronoun,
            "수사" => PartOfSpeechEnum.Numeral,
            "조사" => PartOfSpeechEnum.Particle,
            "동사" => PartOfSpeechEnum.Verb,
            "형용사" => PartOfSpeechEnum.Adjective,
            "관형사" => PartOfSpeechEnum.Determiner,
            "부사" => PartOfSpeechEnum.Adverb,
            "감탄사" => PartOfSpeechEnum.Interjection,
            "접사" => PartOfSpeechEnum.Affix,
            "의존 명사" => PartOfSpeechEnum.BoundNoun,
            "보조 동사" => PartOfSpeechEnum.AuxiliaryVerb,
            "보조 형용사" => PartOfSpeechEnum.AuxiliaryAdjective,
            "어미" => PartOfSpeechEnum.Ending,
            "품사 없음" => PartOfSpeechEnum.NoPartOfSpeech,
            _ => PartOfSpeechEnum.Empty
        };
    }

    public static string ToRussian(this PartOfSpeechEnum pos) => pos switch
    {
        PartOfSpeechEnum.Noun => "существительное",
        PartOfSpeechEnum.Pronoun => "местоимение",
        PartOfSpeechEnum.Numeral => "числительное",
        PartOfSpeechEnum.Particle => "частица",
        PartOfSpeechEnum.Verb => "глагол",
        PartOfSpeechEnum.Adjective => "прилагательное",
        PartOfSpeechEnum.Determiner => "определитель",
        PartOfSpeechEnum.Adverb => "наречие",
        PartOfSpeechEnum.Interjection => "междометие",
        PartOfSpeechEnum.Affix => "аффикс",
        PartOfSpeechEnum.BoundNoun => "зависимое сущ.",
        PartOfSpeechEnum.AuxiliaryVerb => "вспом. глагол",
        PartOfSpeechEnum.AuxiliaryAdjective => "вспом. прилаг.",
        PartOfSpeechEnum.Ending => "окончание",
        PartOfSpeechEnum.NoPartOfSpeech => "без части речи",
        _ => "—"
    };
}
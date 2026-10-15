using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Presentation.Formatting;

public static class EnumDisplayExtensions
{
    private const string NotSpecified = "—";

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
        _ => NotSpecified
    };

    public static string ToRussian(this NiklLevelEnum level) => level switch
    {
        NiklLevelEnum.Beginner => "начальный",
        NiklLevelEnum.Intermediate => "средний",
        NiklLevelEnum.Advanced => "продвинутый",
        _ => NotSpecified
    };
}

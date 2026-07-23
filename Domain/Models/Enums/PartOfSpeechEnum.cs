namespace KoreanLearningApp.Domain.Models.Enums;

public enum PartOfSpeechEnum
{
    Empty = 0,               // 전체
    Noun = 1,                // 명사
    Pronoun = 2,             // 대명사
    Numeral = 3,             // 수사
    Particle = 4,            // 조사
    Verb = 5,                // 동사
    Adjective = 6,           // 형용사
    Determiner = 7,          // 관형사
    Adverb = 8,              // 부사
    Interjection = 9,        // 감탄사
    Affix = 10,              // 접사
    BoundNoun = 11,          // 의존 명사
    AuxiliaryVerb = 12,      // 보조 동사
    AuxiliaryAdjective = 13, // 보조 형용사
    Ending = 14,             // 어미
    NoPartOfSpeech = 15      // 품사 없음
}
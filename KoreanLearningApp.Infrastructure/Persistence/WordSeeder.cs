using KoreanLearningApp.Domain.Models;
namespace KoreanLearningApp.Infrastructure.Persistence;
public static class WordSeeder
{
    public static List<Word> GetSeedWords() => new()
    {
        new Word
        {
            Korean = "안녕하세요",
            TranscriptionRu = "аннёнхасэё",
            TranscriptionEn = "annyeonghaseyo",
            TranslationRu = "Здравствуйте",
            TranslationEn = "Hello",
            RuleExplanation = "안녕하세요 = 안녕(мир/покой) + 하세요 (вежливая форма глагола 하다 — 'делать'). " +
                "Буквально: 'в покое ли вы'. Финаль ㅎ в 하 перед гласной звучит слитно, поэтому 안녕하 " +
                "читается плавно, без резкого 'х'."
        },
        new Word
        {
            Korean = "감사합니다",
            TranscriptionRu = "камсахамнида",
            TranscriptionEn = "gamsahamnida",
            TranslationRu = "Спасибо",
            TranslationEn = "Thank you",
            RuleExplanation = "감사(камса, 'благодарность', ханмунное слово) + 합니다 (формально-вежливое окончание " +
                "глагола 하다). Согласная ㅂ в 합 перед 니 (носовой звук) озвончается по правилу назальной " +
                "ассимиляции, поэтому произносится 'хамнида', а не 'хапнида'."
        },
        new Word
        {
            Korean = "사랑해요",
            TranscriptionRu = "саранхэё",
            TranscriptionEn = "saranghaeyo",
            TranslationRu = "Я люблю тебя",
            TranslationEn = "I love you",
            RuleExplanation = "사랑(любовь) + 해요 (вежливо-неформальная форма от 하다). Финальная ㅎ в 하 перед " +
                "гласной 요 не выпадает, а сохраняется мягким придыханием — поэтому слышится 'хэё', а не 'эё'."
        },
        new Word
        {
            Korean = "미안해요",
            TranscriptionRu = "мианхэё",
            TranscriptionEn = "mianhaeyo",
            TranslationRu = "Извините",
            TranslationEn = "Sorry",
            RuleExplanation = "미안(смятение/неловкость, ханмунное слово) + 해요 (форма от 하다). Буквы 안 и 해 " +
                "не сливаются, каждая читается отдельным слогом: ми-ан-хэ-ё."
        },
        new Word
        {
            Korean = "친구",
            TranscriptionRu = "чхингу",
            TranscriptionEn = "chingu",
            TranslationRu = "Друг",
            TranslationEn = "Friend",
            RuleExplanation = "Буква ㅊ передаёт придыхательный звук 'чх' (сильнее обычного 'ч'), поэтому в " +
                "русской транскрипции пишут 'чх', а не просто 'ч'."
        },
        new Word
        {
            Korean = "가족",
            TranscriptionRu = "каджок",
            TranscriptionEn = "gajok",
            TranslationRu = "Семья",
            TranslationEn = "Family",
            RuleExplanation = "Согласная ㅈ между гласными озвончается и звучит как 'дж' (а не 'ч' или 'ц'), " +
                "поэтому 족 читается 'джок', а не 'чок'."
        },
        new Word
        {
            Korean = "물",
            TranscriptionRu = "муль",
            TranscriptionEn = "mul",
            TranslationRu = "Вода",
            TranslationEn = "Water",
            RuleExplanation = "Финальная согласная ㄹ в конце слога произносится мягко, близко к русскому 'ль', " +
                "а не как твёрдое 'л' — отсюда транскрипция 'муль'."
        },
        new Word
        {
            Korean = "음식",
            TranscriptionRu = "ымсик",
            TranscriptionEn = "eumsik",
            TranslationRu = "Еда",
            TranslationEn = "Food",
            RuleExplanation = "Гласная 으 передаётся русским 'ы', так как в корейском нет отдельного звука, " +
                "точно совпадающего с русским 'у' или 'и' — 으 стоит между ними, ближе к 'ы'."
        },
        new Word
        {
            Korean = "학교",
            TranscriptionRu = "хаккё",
            TranscriptionEn = "hakgyo",
            TranslationRu = "Школа",
            TranslationEn = "School",
            RuleExplanation = "Согласная ㄱ после финального ㄱ (учебная норма 학+교) удваивается по звучанию " +
                "и превращается в напряжённый звук 'кк', поэтому 학교 читается 'хаккё', а не 'хакгё'."
        },
        new Word
        {
            Korean = "사랑",
            TranscriptionRu = "саран",
            TranscriptionEn = "sarang",
            TranslationRu = "Любовь",
            TranslationEn = "Love",
            RuleExplanation = "Финальная согласная ㅇ (ng) в конце слога не выпадает и не переходит на " +
                "следующий слог — это носовой звук 'нг', отдельно стоящий в конце, как в английском 'sing'."
        },
    };
}
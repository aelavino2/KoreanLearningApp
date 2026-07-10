using KoreanLearningApp.Models;

namespace KoreanLearningApp.Services;

public static class WordSeeder
{
    public static List<Word> GetSeedWords() => new()
    {
        new Word { Korean = "안녕하세요", TranscriptionRu = "аннёнхасэё", TranscriptionEn = "annyeonghaseyo", TranslationRu = "Здравствуйте", TranslationEn = "Hello" },
        new Word { Korean = "감사합니다", TranscriptionRu = "камсахамнида", TranscriptionEn = "gamsahamnida", TranslationRu = "Спасибо", TranslationEn = "Thank you" },
        new Word { Korean = "사랑해요", TranscriptionRu = "саранхэё", TranscriptionEn = "saranghaeyo", TranslationRu = "Я люблю тебя", TranslationEn = "I love you" },
        new Word { Korean = "미안해요", TranscriptionRu = "мианхэё", TranscriptionEn = "mianhaeyo", TranslationRu = "Извините", TranslationEn = "Sorry" },
        new Word { Korean = "친구", TranscriptionRu = "чхингу", TranscriptionEn = "chingu", TranslationRu = "Друг", TranslationEn = "Friend" },
        new Word { Korean = "가족", TranscriptionRu = "каджок", TranscriptionEn = "gajok", TranslationRu = "Семья", TranslationEn = "Family" },
        new Word { Korean = "물", TranscriptionRu = "муль", TranscriptionEn = "mul", TranslationRu = "Вода", TranslationEn = "Water" },
        new Word { Korean = "음식", TranscriptionRu = "ымсик", TranscriptionEn = "eumsik", TranslationRu = "Еда", TranslationEn = "Food" },
        new Word { Korean = "학교", TranscriptionRu = "хаккё", TranscriptionEn = "hakgyo", TranslationRu = "Школа", TranslationEn = "School" },
        new Word { Korean = "사랑", TranscriptionRu = "саран", TranscriptionEn = "sarang", TranslationRu = "Любовь", TranslationEn = "Love" },
    };
}
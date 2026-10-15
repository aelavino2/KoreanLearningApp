using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using SQLite;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Запись и удаление графа слова (Words, KrDicts, KrDictSenses, LangInfos, Audios) на синхронном соединении.
/// Вызывается внутри <see cref="SQLiteAsyncConnection.RunInTransactionAsync"/>, чтобы граф сохранялся целиком или не сохранялся вовсе.
/// </summary>
internal static class WordGraphWriter
{
    public static int Insert(SQLiteConnection db, Word word)
    {
        var wordEntity = word.ToEntity();
        db.Insert(wordEntity);
        InsertKrDict(db, word.KrDict, wordEntity.Id);
        return wordEntity.Id;
    }

    public static int Update(SQLiteConnection db, Word word)
    {
        var wordEntity = word.ToEntity();
        wordEntity.Id = word.Id;
        db.Update(wordEntity);
        DeleteKrDicts(db, word.Id);
        InsertKrDict(db, word.KrDict, word.Id);
        return word.Id;
    }

    public static int Delete(SQLiteConnection db, int wordId)
    {
        DeleteKrDicts(db, wordId);
        return db.Delete<WordEntity>(wordId);
    }

    private static void InsertKrDict(SQLiteConnection db, KrDict? krDict, int wordId)
    {
        if (krDict is null)
            return;

        var krDictEntity = krDict.ToEntity(wordId);
        krDictEntity.AudioId = InsertOptional(db, krDict.Audio?.ToEntity(), a => a.Id);
        db.Insert(krDictEntity);

        foreach (var sense in krDict.Senses)
        {
            var senseEntity = sense.ToEntity(krDictEntity.Id);
            senseEntity.EnId = InsertOptional(db, sense.En?.ToEntity(), l => l.Id);
            senseEntity.RuId = InsertOptional(db, sense.Ru?.ToEntity(), l => l.Id);
            db.Insert(senseEntity);
        }
    }

    private static void DeleteKrDicts(SQLiteConnection db, int wordId)
    {
        var krDicts = db.Table<KrDictEntity>().Where(k => k.WordId == wordId).ToList();

        foreach (var krDict in krDicts)
        {
            var senses = db.Table<KrDictSenseEntity>().Where(s => s.KrDictId == krDict.Id).ToList();
            var langInfoIds = senses
                .SelectMany(s => new[] { s.EnId, s.RuId })
                .OfType<int>();

            foreach (var langInfoId in langInfoIds)
                db.Delete<LangInfoEntity>(langInfoId);

            db.Table<KrDictSenseEntity>().Delete(s => s.KrDictId == krDict.Id);

            if (krDict.AudioId is { } audioId)
                db.Delete<AudioEntity>(audioId);
        }

        db.Table<KrDictEntity>().Delete(k => k.WordId == wordId);
    }

    private static int? InsertOptional<T>(SQLiteConnection db, T? entity, Func<T, int> getId)
        where T : class
    {
        if (entity is null)
            return null;

        db.Insert(entity);
        return getId(entity);
    }
}

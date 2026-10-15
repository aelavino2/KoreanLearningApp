---
name: testing
description: Как писать тесты в KoreanLearningApp — только на важное поведение, без дублирования и без тестов ради покрытия. Использовать, когда нужно написать, добавить, дополнить или проревьюить тесты, создать тестовый проект, решить, что стоит тестировать, или проверить изменение тестами (xUnit, NSubstitute, SQLite, ViewModel'и MAUI).
---

# Тестирование в KoreanLearningApp

Главный принцип: **тест пишется на поведение, поломка которого ударит по пользователю или данным.** Не на каждый метод, не на каждую ветку, не ради процента покрытия. Один хороший тест на правило лучше пяти, повторяющих друг друга.

## 1. Что тестировать (по приоритету)

1. **Целостность данных** — то, что пишет и читает БД:
   - `WordRepository.SaveWordAsync` / `DeleteWordAsync`: слово с `KrDict`, `Senses`, `En/Ru`, `Audio` сохраняется и читается обратно целиком; при удалении/пересохранении не остаётся «сирот» в `KrDictSenses`, `LangInfos`, `Audios`.
   - `WordImportRepository.InsertManyAsync` + `GetExistingKeysAsync`.
2. **Импорт / экспорт** — пользователь не должен потерять или задублировать слова:
   - повторный импорт того же JSON добавляет 0 слов (ключ `Korean_SupNo`);
   - `KrDictJsonSerializer`: массив, несколько корневых объектов подряд, пустая строка;
   - round-trip: export → import даёт эквивалентные слова;
   - `WordMappingConfig`: `Korean` берётся из `KrDict.Word`, если он есть; нечисловой `Rank` → 0.
3. **Поиск и пагинация** — `GetWordsPageQuery`: поиск по корейскому, RU- и EN-переводу; слово с несколькими значениями не дублируется (`DISTINCT`) и считается в `TotalCount` один раз; порядок по `Rank`; корректный `OFFSET` на 2-й странице.
4. **Конвертация enum ↔ строки** — `PartOfSpeechMapper`, `NiklLevelMapper`, `TopikLevelMapper`: один round-trip тест по **всем** значениям enum (`Enum.GetValues`), плюс неизвестная строка → `Empty`. Не по тесту на каждое значение.
5. **Логика ViewModel**, у которой есть правила:
   - `WordsViewModel`: `TotalPages` из `totalCount`, «далее» доступно только после `ScrolledToEnd` и не на последней странице, смена поиска сбрасывает на 1-ю страницу;
   - `WordDetailViewModel`: `PrimaryTranslation` (EN, иначе RU), `HasAudio` управляет `PlayAudioCommand.CanExecute`.

## 2. Что НЕ тестировать

- Авто-свойства, конструкторы, DTO/Entity без логики.
- Код, сгенерированный CommunityToolkit (`[ObservableProperty]` поднимает `PropertyChanged` — это тестирует сам тулкит).
- Сквозные обёртки без логики (`WordService` просто делегирует в репозиторий — тест на него проверяет только мок).
- Регистрацию DI, XAML-разметку, стили, `ShellNavigationService` (тонкая обёртка над `Shell`).
- `BaseRepository<T>` отдельно — он покрывается тестами репозиториев на реальном SQLite.
- Одно и то же правило на нескольких уровнях. Если дедупликация проверена в `WordImportService`, не повторяй её в тесте репозитория и ViewModel.

## 3. Как писать

**Стек:** xUnit, NSubstitute для моков интерфейсов, обычные `Assert.*` (FluentAssertions v8 платный — не добавлять без согласования).

**Структура проекта тестов** (создать, когда понадобится первый тест):

```
tests/KoreanLearningApp.Tests/KoreanLearningApp.Tests.csproj   (net9.0)
  → ProjectReference: Domain, Services, Infrastructure
```

```bash
dotnet new xunit -o tests/KoreanLearningApp.Tests -f net9.0
dotnet sln KoreanLearningApp.sln add tests/KoreanLearningApp.Tests/KoreanLearningApp.Tests.csproj
dotnet add tests/KoreanLearningApp.Tests package NSubstitute
dotnet test tests/KoreanLearningApp.Tests
```

ViewModel'и живут в MAUI-проекте с платформенными TFM (`net10.0-android` и т.д.), обычный тестовый проект на него сослаться не может. Варианты (обсудить с пользователем перед выбором): вынести ViewModel'и в отдельную библиотеку `KoreanLearningApp.Presentation` (net10.0, `UseMaui` не нужен), либо добавить в MAUI-проект TFM `net10.0` с `OutputType=Library` для этого TFM. До этого — сначала убрать из тестируемых ViewModel прямые вызовы `Shell.Current`/`FilePicker` за интерфейсы.

**Правила теста:**

- Имя: `Метод_Условие_ОжидаемыйРезультат` — `ImportFromJsonAsync_SameJsonTwice_ImportsOnlyOnce`.
- Arrange / Act / Assert, разделённые пустой строкой. Один Act на тест.
- Проверяй **результат и состояние**, а не вызовы. `Received()` — только когда вызов и есть результат (например, «аудио не запускается, если уже играет»).
- Мокай только границы: репозитории для сервисов, сервисы для ViewModel. **Не мокай SQLite** — для инфраструктуры используй настоящую БД во временном файле:
  ```csharp
  public sealed class SqliteFixture : IDisposable
  {
      public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_{Guid.NewGuid():N}.db");
      public DbContext Db => _db ??= new DbContext(Path);
      private DbContext? _db;
      public void Dispose() { SQLiteAsyncConnection.ResetPool(); File.Delete(Path); }
  }
  ```
  Один файл БД на тест (через `IDisposable` в классе теста), чтобы тесты не зависели друг от друга.
- Перед тестами Mapster вызывай `WordMappingConfig.Configure()` (идемпотентен).
- Тестовые данные — через маленький билдер/фабрику (`TestWords.WithSenses("사과", ru: "яблоко", en: "apple")`), а не копипаст графа объектов в каждом тесте. Только поля, важные для этого теста.
- `[Theory]` + `[InlineData]` — когда это действительно одно правило на разных входах. Не плоди `Theory` с десятью строками, проверяющими одну и ту же ветку.
- Никаких `Task.Delay`/`Thread.Sleep` в тестах. Для debounce в `WordsViewModel` проверяй публичную команду `LoadWordsAsync`, а не ожидание таймера.
- Тест детерминирован: без `DateTime.Now`, сети, реальных файлов вне временной папки.

## 4. Порядок работы

1. Сформулируй 1–5 правил поведения, которые изменение может сломать. Если правило не сформулировать одним предложением — тест, скорее всего, не нужен.
2. Проверь, не покрыто ли правило уже существующим тестом (`Grep` по имени метода в `tests/`).
3. Напиши минимальный набор тестов; для бага — сначала падающий тест, воспроизводящий его.
4. Запусти `dotnet test` и покажи пользователю реальный результат (сколько прошло/упало).
5. В отчёте перечисли, что сознательно **не** тестировал и почему — коротко.

## Пример хорошего теста

```csharp
public sealed class WordImportServiceTests : IDisposable
{
    private readonly SqliteFixture _sqlite = new();

    [Fact]
    public async Task ImportFromJsonAsync_SameJsonTwice_ImportsOnlyOnce()
    {
        WordMappingConfig.Configure();
        var service = TestServices.CreateImportService(_sqlite.Db);
        const string json = """[{ "word": "사과", "krdict": { "word": "사과", "sup_no": 1 } }]""";

        var first = await service.ImportFromJsonAsync(json);
        var second = await service.ImportFromJsonAsync(json);

        Assert.Equal(1, first);
        Assert.Equal(0, second);
    }

    public void Dispose() => _sqlite.Dispose();
}
```

(Имена полей JSON сверяй с `Infrastructure/Import/DTO/*JsonDto.cs` перед использованием.)

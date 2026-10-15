---
name: testing
description: Как писать тесты в KoreanLearningApp — только на важное поведение, без дублирования и без тестов ради покрытия. Использовать, когда нужно написать, добавить, дополнить или проревьюить тесты, создать тестовый проект, решить, что стоит тестировать, или проверить изменение тестами (xUnit, NSubstitute, SQLite, ViewModel'и в Presentation).
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
5. **Практика** — порядок отбора слов (сохранённые → к повторению → новые, без дублей), фильтр TOPIK, слово после верного ответа не предлагается сегодня, интервалы SM-2, варианты ответа квиза.
6. **Логика ViewModel** (проект Presentation), у которой есть правила:
   - `WordsViewModel`: `TotalPages` из `totalCount`, «далее» доступно только после `ScrolledToEnd` и не на последней странице;
   - `WordDetailViewModel`: `PrimaryTranslation` (EN, иначе RU), `HasAudio` управляет `PlayAudioCommand.CanExecute`;
   - `PracticeViewModel`: очки и оценка за ответ, истечение таймера = Forgot, завершение сессии после последнего вопроса.

## 2. Что НЕ тестировать

- Авто-свойства, конструкторы, DTO/Entity без логики.
- Код, сгенерированный CommunityToolkit (`[ObservableProperty]` поднимает `PropertyChanged` — это тестирует сам тулкит).
- Сквозные обёртки без логики (`WordService` просто делегирует в репозиторий — тест на него проверяет только мок).
- Регистрацию DI, XAML-разметку, стили, `ShellNavigationService` (тонкая обёртка над `Shell`).
- `BaseRepository<T>` отдельно — он покрывается тестами репозиториев на реальном SQLite.
- Одно и то же правило на нескольких уровнях. Если дедупликация проверена в `WordImportService`, не повторяй её в тесте репозитория и ViewModel.

## 3. Как писать

**Стек:** xUnit, NSubstitute для моков интерфейсов, обычные `Assert.*` (FluentAssertions v8 платный — не добавлять без согласования).

**Проект тестов:** `tests/KoreanLearningApp.Tests` (net10.0) → Domain, Services, Infrastructure, Presentation. Папки повторяют области: `Persistence/`, `Import/`, `Practice/`, `ViewModels/`, общие помощники — `Support/`.

```bash
dotnet test tests/KoreanLearningApp.Tests
```

Помощники в `Support/` — используй их, не пиши свои:
- `SqliteDatabase` — временный файл БД на тест, `CountAsync(table)` для проверки «сирот»;
- `TestServices` — реальные репозитории и сервисы поверх тестовой БД (как в `MauiProgram`);
- `TestWords.Create(...)` — слово с нужными полями;
- `WaitUntilAsync(condition)` — ожидание по `PropertyChanged` для логики, которая продолжается после таймера.

ViewModel'и лежат в `KoreanLearningApp.Presentation` (без MAUI), поэтому тестируются напрямую: границы (`I*Service`, `INavigationService`, `IDialogService`, `IFileService`) мокаются через NSubstitute, время — `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`) и `Advance(...)`.

**Правила теста:**

- Имя: `Метод_Условие_ОжидаемыйРезультат` — `ImportFromJsonAsync_SameJsonTwice_ImportsOnlyOnce`.
- Arrange / Act / Assert, разделённые пустой строкой. Один Act на тест.
- Проверяй **результат и состояние**, а не вызовы. `Received()` — только когда вызов и есть результат (например, «аудио не запускается, если уже играет»).
- Мокай только границы: репозитории/сервисы для ViewModel. **Не мокай SQLite** — для Infrastructure и сервисов практики используй `SqliteDatabase` + `TestServices`. Класс теста реализует `IAsyncLifetime` и в `DisposeAsync` вызывает `_database.DisposeAsync()`.
- **Не вызывай `SQLiteAsyncConnection.ResetPool()`**: он глобальный и рвёт соединения тестов, которые xUnit выполняет параллельно.
- `TestServices` сам вызывает `WordMappingConfig.Configure()`.
- Тестовые данные — через `TestWords.Create(...)`, а не копипаст графа объектов. Только поля, важные для этого теста.
- `[Theory]` + `[InlineData]` — когда это действительно одно правило на разных входах. Не плоди `Theory` с десятью строками, проверяющими одну и ту же ветку.
- Никаких `Task.Delay`/`Thread.Sleep` в тестах. Время двигай через `FakeTimeProvider.Advance`, асинхронное продолжение жди через `TaskCompletionSource` из мока или `WaitUntilAsync`. Для debounce в `WordsViewModel` проверяй команду `LoadWordsAsync`, а не таймер.
- Тест детерминирован: без `DateTime.Now`, сети, реальных файлов вне временной папки.

## 4. Порядок работы

1. Сформулируй 1–5 правил поведения, которые изменение может сломать. Если правило не сформулировать одним предложением — тест, скорее всего, не нужен.
2. Проверь, не покрыто ли правило уже существующим тестом (`Grep` по имени метода в `tests/`).
3. Напиши минимальный набор тестов; для бага — сначала падающий тест, воспроизводящий его.
4. Запусти `dotnet test` и покажи пользователю реальный результат (сколько прошло/упало).
5. В отчёте перечисли, что сознательно **не** тестировал и почему — коротко.

## Пример хорошего теста

```csharp
public sealed class WordImportExportTests : IAsyncLifetime
{
    private readonly SqliteDatabase _database = new();
    private readonly TestServices _services;

    public WordImportExportTests()
    {
        _services = new TestServices(_database.Context);
    }

    [Fact]
    public async Task ImportFromJsonAsync_SameJsonTwice_ImportsOnlyOnce()
    {
        const string json = """[{ "word": "사과", "krdict": { "word": "사과", "sup_no": 1 } }]""";

        var first = await _services.ImportService.ImportFromJsonAsync(json);
        var second = await _services.ImportService.ImportFromJsonAsync(json);

        Assert.Equal(1, first);
        Assert.Equal(0, second);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _database.DisposeAsync();
}
```

(Имена полей JSON сверяй с `Infrastructure/Import/DTO/*JsonDto.cs` перед использованием.)

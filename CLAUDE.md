# KoreanLearningApp

Кроссплатформенное приложение (.NET MAUI) для изучения корейских слов: словарь с поиском и пагинацией, карточка слова с переводами (EN/RU) и аудио, импорт/экспорт словаря в JSON, квиз с интервальными повторениями (SM-2) и сохранённые слова.

Общение и тексты UI — на русском (часть заголовков в XAML пока на английском).

## Структура решения

| Проект | TFM | Назначение | Зависит от |
|---|---|---|---|
| `KoreanLearningApp.Domain` | net10.0 | Модели (`Word`, `KrDict`, `Sense`, `WordProgress`, `QuizQuestion`…), enum'ы, `TopikLevelMapper` | — |
| `KoreanLearningApp.Services` | net10.0 | Интерфейсы сервисов и репозиториев (`Abstractions/`), `WordService`, `SavedWordsService`, практика (`Services/Practice`: SM-2, сборка квиза, очки), `AudioPlayerService` | Domain, Plugin.Maui.Audio |
| `KoreanLearningApp.Infrastructure` | net10.0 | SQLite (sqlite-net-pcl), Entities, репозитории, запросы, импорт/экспорт JSON, маппинг | Domain, Services, Mapster |
| `KoreanLearningApp.Presentation` | net10.0 | ViewModel'и (CommunityToolkit.Mvvm) и UI-абстракции: `INavigationService`, `AppRoutes`, `IDialogService`, `IFileService`, форматирование enum'ов (`ToRussian`). **Без MAUI** | Domain, Services |
| `KoreanLearningApp` | net10.0-android/ios/maccatalyst/windows | MAUI: Views (XAML), реализации `ShellNavigationService`/`ShellDialogService`/`MauiFileService`, конвертеры, стили, DI в `MauiProgram.cs` | Presentation, Infrastructure, Services |
| `tests/KoreanLearningApp.Tests` | net10.0 | xUnit: репозитории на реальном SQLite, импорт/экспорт, практика, ViewModel'и | Presentation, Infrastructure, Services |

Направление зависимостей: UI → Presentation / Infrastructure → Services → Domain. Интерфейсы репозиториев лежат в **Services** (`Services/Abstractions/Repositories`), реализации — в Infrastructure. Platform API (Shell, FilePicker, Share, FileSystem) — только в MAUI-проекте за интерфейсами из Presentation.

## Ключевые потоки

- **Хранилище**: `DbContext` лениво открывает `SQLiteAsyncConnection` и создаёт таблицы. Слово хранится нормализованно в 5 таблицах: `Words → KrDicts (WordId) → KrDictSenses (KrDictId) → LangInfos (EnId/RuId)`, `KrDicts.AudioId → Audios`. Сборка доменного `Word` — `WordRepository.BuildWordsAsync` (батч-загрузка по Id, без N+1). Практика: `WordProgresses`, `ReviewLogs`, `SavedWords`.
- **Пагинация/поиск**: `GetWordsPageQuery` — сырой SQL, `LIKE` по `Korean`, RU- и EN-переводам, сортировка по `Rank`. `WordsViewModel` — debounce 300 мс, `PageSize = 100`, кнопка «далее» активна только после прокрутки до конца страницы.
- **Импорт**: JSON (массив или несколько корневых объектов подряд) → `KrDictJsonSerializer` → `WordJsonDto` → Mapster (`WordMappingConfig`) → `Word`. Дубликаты отсекаются по ключу `"{Korean}_{SupNo}"` (`WordImportService` и `WordImportRepository` строят ключ одинаково — менять синхронно).
- **Практика**: `WordPracticeService.CreateQuizAsync` — сохранённые слова → слова к повторению (`NextReviewDate <= now`) → новые по рангу; `QuizQuestionFactory` добавляет до 3 неправильных вариантов; ответ → `QuizScoringService` (оценка) → `SubmitReviewAsync` (SM-2 + журнал). Время — через `TimeProvider`.
- **Enum ↔ строки**: в БД и JSON части речи/уровни хранятся корейскими строками; конвертация — `Infrastructure/Mapster/*Mapper.cs` (`Parse`, `ToKorean`) и `*JsonConverter.cs`, коды TOPIK — `Domain/.../TopikLevelMapper`, русские подписи — `Presentation/Formatting/EnumDisplayExtensions`.
- **Навигация**: `INavigationService` / `ShellNavigationService`; параметр детальной страницы — ключ `NavigationParameters.Word`, его принимает `WordDetailPage` (`[QueryProperty]`) и передаёт во ViewModel.

## Сборка, запуск, тесты

```bash
dotnet build KoreanLearningApp/KoreanLearningApp.csproj -f net10.0-windows10.0.19041.0
dotnet run --project KoreanLearningApp/KoreanLearningApp.csproj -f net10.0-windows10.0.19041.0
dotnet test tests/KoreanLearningApp.Tests
```

БД: `%LOCALAPPDATA%\koreanApp\KoreanLearning.db`. Пути — `KoreanLearningApp/Diagnostics/AppPaths`.

Android-сборка падает с `APT2265` из-за кириллицы в пути `D:\Проекты`: собирать из копии с ASCII-путём (`robocopy ... /XD bin obj .git .claude`), установка — `dotnet build -f net10.0-android -t:Install` при запущенном эмуляторе `pixel_7_-_api_36`.

## Диагностика

- **Логи** — `%LOCALAPPDATA%\koreanApp\logs\app-yyyyMMdd.log` (Android: `adb shell "run-as com.companyname.koreanlearningapp cat files/koreanApp/logs/app-yyyyMMdd.log"`). Пишет `FileLoggerProvider` (уровень Information и выше).
- **Клики и навигация** — `UiInteractionLogger` подписывается на все кнопки, тапы, чекбоксы, переключатели, радиокнопки, пикеры и `Shell.Navigated` в окне. Строка лога: действие, элемент, текст, страница, тип BindingContext. Чтобы понять, что произошло после клика, читай лог.
- **Ошибки** — `GlobalExceptionHandler` (регистрируется в `App`): необработанные исключения UI на Windows/Android пишутся в лог и показываются пользователю сообщением вместо падения.
- Проверять UI на Windows симуляцией настоящей мыши нельзя (клики могут уйти в чужое окно); на Android — `adb shell input tap` и `adb shell screencap`.

## Соглашения

- Стиль кода и MAUI/MVVM-правила — скилл `.claude/skills/maui-clean-code`.
- Что и как тестировать — скилл `.claude/skills/testing`.
- File-scoped namespace, primary constructors для DI, `[ObservableProperty]` / `[RelayCommand]` из CommunityToolkit.Mvvm, compiled bindings (`x:DataType`) в каждой странице и `DataTemplate`.
- Комментарии в коде — только `/// <summary>`, обычные — лишь при необходимости.
- Цвета и стили — только через ресурсы `Resources/Styles/AppColors.xaml` и `AppStyles.xaml` (`{StaticResource ...}`), без хардкода в страницах.
- Новая страница = View (MAUI-проект) + ViewModel (Presentation) + регистрация обоих в `MauiProgram.cs` (+ `Routing.RegisterRoute` для не-табовых страниц) + константа в `AppRoutes`.

## Известный технический долг (не чинить попутно без запроса)

- Namespace не совпадает с папкой: `PartOfSpeechMapper` (в `Domain.Models.Enums`, лежит в Infrastructure), `EntityMappingExtensions` (`Infrastructure.Persistence.Mapster`), `DatabaseConstants` (`Infrastructure.Persistence`); `KrDict` вообще без namespace.
- `AudioPlayerService` в Services использует MAUI Essentials (`FileSystem`) и `DevPaths` (dev-хак для Windows Debug).
- `KrDictJsonDto.Pos` использует `JsonStringEnumConverter` (ждёт `"Noun"`), а `WordJsonDto.PartOfSpeech` — `PartOfSpeechJsonConverter` (корейская строка `"명사"`). Проверить формат исходного JSON.
- Множественные INSERT при импорте/сохранении идут без транзакции.
- В `KoreanLearningApp.csproj` остались ссылки на удалённый `Views/Control/NavigationBarView.xaml`.

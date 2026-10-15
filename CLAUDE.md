# KoreanLearningApp

Кроссплатформенное приложение (.NET MAUI) для изучения корейских слов: словарь с поиском и пагинацией, карточка слова с переводами (EN/RU) и аудио, импорт/экспорт словаря в JSON. В разработке: вкладки Practice и Saved (роуты уже объявлены в `Navigation/AppRoutes.cs`).

Общение и тексты UI — на русском (часть заголовков в XAML пока на английском).

## Структура решения

| Проект | TFM | Назначение | Зависит от |
|---|---|---|---|
| `KoreanLearningApp.Domain` | net9.0 | Модели (`Word`, `KrDict`, `Sense`, `LangInfo`, `Audio`) и enum'ы | — |
| `KoreanLearningApp.Services` | net9.0 | Интерфейсы сервисов и репозиториев (`Abstractions/`), `WordService`, `AudioPlayerService` | Domain, Plugin.Maui.Audio |
| `KoreanLearningApp.Infrastructure` | net9.0 | SQLite (sqlite-net-pcl), Entities, репозитории, запросы, импорт/экспорт JSON, маппинг | Domain, Services, Mapster |
| `KoreanLearningApp` | net10.0-android/ios/maccatalyst/windows | MAUI UI: Views (XAML), ViewModels (CommunityToolkit.Mvvm), Shell-навигация, DI в `MauiProgram.cs` | Infrastructure, Services |

Направление зависимостей: UI → Infrastructure → Services → Domain. Интерфейсы репозиториев лежат в **Services** (`Services/Abstractions/Repositories`), реализации — в Infrastructure.

## Ключевые потоки

- **Хранилище**: `DbContext` лениво открывает `SQLiteAsyncConnection` и создаёт таблицы. Слово хранится нормализованно в 5 таблицах: `Words → KrDicts (WordId) → KrDictSenses (KrDictId) → LangInfos (EnId/RuId)`, `KrDicts.AudioId → Audios`. Сборка доменного `Word` — `WordRepository.BuildWordsAsync` (батч-загрузка по Id, без N+1).
- **Пагинация/поиск**: `GetWordsPageQuery` — сырой SQL, `LIKE` по `Korean`, RU- и EN-переводам, сортировка по `Rank`. `WordsViewModel` — debounce 300 мс, `PageSize = 100`, кнопка «далее» активна только после прокрутки до конца страницы.
- **Импорт**: JSON (массив или несколько корневых объектов подряд) → `KrDictJsonSerializer` → `WordJsonDto` → Mapster (`WordMappingConfig`) → `Word`. Дубликаты отсекаются по ключу `"{Korean}_{SupNo}"` (`WordImportService` и `WordImportRepository` строят ключ одинаково — менять синхронно).
- **Enum ↔ строки**: в БД и JSON части речи/уровни хранятся корейскими строками; конвертация в `Infrastructure/Mapster/*Mapper.cs` (`Parse`, `ToKorean`, `ToRussian`, `ToCode`) и `*JsonConverter.cs`.
- **Навигация**: `INavigationService` / `ShellNavigationService`; параметр детальной страницы передаётся ключом `"Word"` (`[QueryProperty(nameof(Word), "Word")]`). Детальный роут — `AppRoutes.WordDetail`.

## Сборка и запуск

```bash
dotnet build KoreanLearningApp.sln
dotnet build KoreanLearningApp/KoreanLearningApp.csproj -f net10.0-windows10.0.19041.0
dotnet run --project KoreanLearningApp/KoreanLearningApp.csproj -f net10.0-windows10.0.19041.0
```

БД: `%LOCALAPPDATA%\koreanApp\KoreanLearning.db`. Тестового проекта пока нет — см. скилл `testing`.

## Соглашения

- Стиль кода и MAUI/MVVM-правила — скилл `.claude/skills/maui-clean-code`.
- Что и как тестировать — скилл `.claude/skills/testing`.
- File-scoped namespace, primary constructors для DI, `[ObservableProperty]` / `[RelayCommand]` из CommunityToolkit.Mvvm, compiled bindings (`x:DataType`) в каждой странице и `DataTemplate`.
- Цвета и стили — только через ресурсы `Resources/Styles/AppColors.xaml` и `AppStyles.xaml` (`{StaticResource ...}`), без хардкода в страницах.
- Новая страница = View + ViewModel + регистрация обоих в `MauiProgram.cs` (+ `Routing.RegisterRoute` для не-табовых страниц) + константа в `AppRoutes`.

## Известный технический долг (не чинить попутно без запроса)

- `IWordImportService` зарегистрирован в DI дважды (`MauiProgram.cs`).
- Namespace не совпадает с папкой: `PartOfSpeechMapper` (в `Domain.Models.Enums`, лежит в Infrastructure), `EntityMappingExtensions` (`Infrastructure.Persistence.Mapster`), `DatabaseConstants` (`Infrastructure.Persistence`); `KrDict` вообще без namespace.
- ViewModel'и напрямую вызывают `Shell.Current.DisplayAlert` — это мешает unit-тестам; новый код должен идти через абстракцию диалогов.
- `AudioPlayerService` в Services использует MAUI Essentials (`FileSystem`) и `DevPaths` (dev-хак для Windows Debug).
- `KrDictJsonDto.Pos` использует `JsonStringEnumConverter` (ждёт `"Noun"`), а `WordJsonDto.PartOfSpeech` — `PartOfSpeechJsonConverter` (корейская строка `"명사"`). Проверить формат исходного JSON.
- Множественные INSERT при импорте/сохранении идут без транзакции.
- В `KoreanLearningApp.csproj` остались ссылки на удалённый `Views/Control/NavigationBarView.xaml`.

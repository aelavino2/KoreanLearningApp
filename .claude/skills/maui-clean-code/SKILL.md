---
name: maui-clean-code
description: Правила чистого кода на C# для .NET MAUI (XAML + MVVM на CommunityToolkit.Mvvm) в KoreanLearningApp. Использовать при написании или рефакторинге любого C#/XAML кода в проекте — ViewModel, страниц, сервисов, репозиториев, конвертеров, стилей, DI-регистраций — и при ревью таких изменений.
---

# Чистый код C# / .NET MAUI в KoreanLearningApp

Цель — код, который читается сверху вниз, легко тестируется и не протаскивает платформу MAUI туда, где ей не место. Перед правкой посмотри соседние файлы и пиши так же, как они.

## 1. Слои и зависимости

```
KoreanLearningApp (UI) → Infrastructure → Services → Domain
```

- **Domain** — только POCO и enum'ы. Никаких атрибутов SQLite, JSON, MAUI.
- **Services** — интерфейсы (`Abstractions/Services`, `Abstractions/Repositories`) и бизнес-логика. Не знает про SQLite, Mapster, Shell, `DisplayAlert`.
- **Infrastructure** — реализации репозиториев, SQL-запросы, Entities, DTO, маппинг, импорт/экспорт.
- **UI** — Views, ViewModels, конвертеры, навигация, DI. ViewModel зависит только от интерфейсов из Services и от UI-абстракций (`INavigationService`, диалоги).
- Новая зависимость вверх по стрелке (Domain → Services и т.п.) — запрещена.
- Platform API (`FileSystem`, `FilePicker`, `Share`, `Shell.Current`, `MainThread`) — только за интерфейсом в UI-слое, чтобы ViewModel можно было протестировать.

## 2. Общий стиль C#

- File-scoped namespace (`namespace X;`), namespace = путь к папке.
- Primary constructor для внедрения зависимостей: `public class WordService(IWordRepository repository) : IWordService`.
- `sealed` для классов, от которых не наследуются (сервисы, конвертеры JSON).
- `var`, когда тип очевиден из правой части; иначе явный тип.
- Nullable включён: не глуши предупреждения `!` без причины, обрабатывай `null` явно (`is null`, `??`, ранний `return`).
- Ранний выход вместо вложенных `if`:
  ```csharp
  if (word is null)
      return;
  ```
- Магические числа и строки — в `const` (`PageSize`, `SearchDebounceMs`) или `AppRoutes`.
- Имена: `PascalCase` для типов/методов/свойств, `_camelCase` для приватных полей, суффикс `Async` у асинхронных методов, интерфейсы с `I`.
- Метод делает одно дело и помещается на экран. Повторяющийся блок (например, вставка `KrDict` + `Senses` + `LangInfo` в `WordRepository` и `WordImportRepository`) — выносить в общий метод, а не копировать в третий раз.
- Не оставляй неиспользуемые `using`, закомментированный код, пустые `catch {}`.

### Комментарии

- Комментарии в коде **не приветствуются**. Код должен объяснять себя сам: имена методов, переменных, маленькие методы.
- Допустим только XML-doc `/// <summary>` — на публичных интерфейсах, сервисах и методах, где контракт неочевиден из сигнатуры.
- Обычный `//` комментарий — только по необходимости: неочевидное «почему» (обход бага платформы, намеренно проглоченное исключение, формула из внешнего источника). Никаких комментариев «что делает следующая строка», разделителей и TODO без задачи.
- В XAML — тоже без комментариев; смысл выражают имена стилей и свойств ViewModel.

## 3. Асинхронность

- `async Task`, никогда `async void` — кроме обработчиков событий/override вроде `OnAppearing` (там оборачивай в try/catch или вызывай команду).
- Не блокируй: никаких `.Result`, `.Wait()`, `GetAwaiter().GetResult()`.
- Долгие операции принимают `CancellationToken` и пробрасывают его дальше.
- Fire-and-forget (`_ = DoAsync()`) — только с обработкой исключений внутри и отменой предыдущего вызова (паттерн debounce в `WordsViewModel.OnSearchTextChanged`).
- Если метод не делает `await`, не помечай его `async` — верни `Task` напрямую (`=> repository.GetWordsAsync()`).

## 4. MVVM на CommunityToolkit.Mvvm

```csharp
public partial class WordDetailViewModel(INavigationService navigationService, IAudioPlayerService audioPlayerService)
    : ObservableObject
{
    [ObservableProperty]
    private bool _isAudioBusy;

    public bool HasAudio => !string.IsNullOrWhiteSpace(Word.KrDict?.Audio?.Url);

    [RelayCommand(CanExecute = nameof(HasAudio))]
    private async Task PlayAudioAsync() { ... }
}
```

- Класс `partial`, наследник `ObservableObject`.
- Состояние — `[ObservableProperty] private T _field;`. Не пиши `SetProperty` руками, если генератор справляется.
- Вычисляемые свойства — `=>`-свойства; уведомление о них — через `[NotifyPropertyChangedFor(nameof(X))]` на источнике (предпочтительно) или в `partial void OnXChanged`.
- Действия — `[RelayCommand]` на приватном методе. Доступность — `CanExecute = nameof(...)` + `[NotifyCanExecuteChangedFor]`, а не проверка флага в начале метода и `IsEnabled` в XAML.
- Флаги загрузки (`IsLoading`, `IsBusy`) — ставь в `try`, снимай в `finally`.
- ViewModel не знает про View: никаких ссылок на `Page`, контролы, `x:Name`.
- Не вызывай `Shell.Current.DisplayAlert` из ViewModel в новом коде — используй интерфейс диалогов (например `IDialogService.ShowAlertAsync(title, message)`), реализованный в UI-слое.
- Элементы списков — отдельные лёгкие ViewModel (`WordItemViewModel`, `SenseItemViewModel`), а не доменные модели напрямую, если нужно форматирование для отображения.
- Коллекции для UI — `ObservableCollection<T>` с `{ get; }` (без сеттера); заменять содержимое через `Clear` + `Add`, а не пересоздавать объект.
- Параметры навигации — `[QueryProperty]` или `IQueryAttributable`; ключ — константа, а не строка в двух местах.

## 5. Views (XAML)

- В каждой странице и каждом `DataTemplate` — `x:DataType` (compiled bindings; проект собирается с `MauiXamlInflator=SourceGen`).
- Code-behind содержит только: `InitializeComponent()`, `BindingContext = viewModel` из конструктора и чисто визуальную логику (как `OnSizeAllocated` с `ResponsiveLayout`). Бизнес-логика, загрузка данных, навигация — в ViewModel. Загрузку при появлении страницы лучше привязать через `EventToCommandBehavior` (CommunityToolkit.Maui) или вызвать команду.
- Цвета, размеры шрифтов, отступы карточек — только `{StaticResource ...}` из `AppColors.xaml` / `AppStyles.xaml`. Повторившийся дважды набор сеттеров → именованный `Style` в `AppStyles.xaml`.
- Тексты для пользователя — на русском и в одном месте (константы/ресурсы), а не разбросаны по XAML и C#.
- Лейауты: `Grid` с `RowDefinitions="Auto,*"` вместо вложенных `StackLayout`; не вкладывай `CollectionView` в `ScrollView`; `VerticalStackLayout`/`HorizontalStackLayout` вместо устаревшего `StackLayout`.
- Видимость — `IsVisible="{Binding HasX}"` с bool-свойством во ViewModel, а не конвертер на каждую мелочь. Конвертер — только если логика переиспользуется (`InverseBoolConverter`).
- Списки: `CollectionView` с `SelectionMode="None"` + `TapGestureRecognizer`/команда, `RemainingItemsThresholdReachedCommand` для догрузки.

## 6. Навигация и DI

- Навигация только через `INavigationService`; роуты — константы `AppRoutes`.
- Страницы и ViewModel — `AddTransient`; сервисы без состояния, репозитории, `DbContext` — `AddSingleton`.
- Каждый интерфейс регистрируется один раз. Группируй регистрации по слоям (можно вынести в extension-методы `AddInfrastructure()`, `AddViewModels()`).
- Не используй `ServiceLocator`/`IPlatformApplication.Current.Services` внутри классов — только внедрение через конструктор.

## 7. Данные и инфраструктура

- Entities (SQLite) ≠ доменные модели: маппинг через `EntityMappingExtensions` (`ToEntity`/`ToDomain`), DTO ↔ Domain — через Mapster (`WordMappingConfig`).
- Строки ↔ enum — через соответствующий `*Mapper` (`Parse`, `ToKorean`, `ToRussian`), а не `switch` по месту.
- SQL — параметризованный (`?`), никогда не конкатенация пользовательского ввода.
- Батч-загрузка вместо запросов в цикле (см. `WordRepository.BuildWordsAsync`). Пачку INSERT'ов — в транзакцию (`RunInTransactionAsync`).
- Ключ дедупликации слова `"{Korean}_{SupNo}"` должен строиться в одном месте.

## 8. Ошибки

- Ловить исключения на границе UI (команда ViewModel) и показывать пользователю понятное сообщение; ниже по стеку — не ловить без возможности что-то исправить.
- Не ловить `Exception`, если можно поймать конкретный тип (`IOException`, `JsonException`, `HttpRequestException`).
- Не показывать пользователю `ex.Message` как единственный текст для ожидаемых ошибок (некорректный JSON, нет файла) — сформулируй по-человечески.

## Чеклист перед завершением

- [ ] Слои не нарушены; ViewModel не трогает `Shell.Current`/platform API напрямую в новом коде.
- [ ] Все bindings компилируемые (`x:DataType`), нет хардкода цветов.
- [ ] Новые View/ViewModel/сервисы зарегистрированы в `MauiProgram.cs`, роут — в `AppRoutes`.
- [ ] Нет `async void` (кроме обработчиков), `.Result`, пустых `catch`.
- [ ] Нет дублирования логики, которую уже делает существующий маппер/репозиторий.
- [ ] `dotnet build KoreanLearningApp.sln` проходит без новых предупреждений.

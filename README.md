# Korean Learning App

Приложение на .NET MAUI для изучения корейских слов: словарь с переводами на русский и английский, квиз с интервальными повторениями и сохранённые слова. Работает на Android и Windows (iOS и macOS собираются, но не проверялись).

## Возможности

- **Словарь** — поиск по корейскому, русскому и английскому; постраничный просмотр или лента с подгрузкой.
- **Карточка слова** — значения, произношение, ханча, грамматика, аудио.
- **Квиз** — вопросы «корейский → перевод» и наоборот, таймер, очки; повторения планируются по алгоритму SM-2.
- **Сохранённые слова** — отдельная вкладка, могут идти первыми в квизе.
- **Импорт и экспорт** словаря в JSON.

## Структура

```
src/
  KoreanLearningApp/                 MAUI-приложение: страницы, стили, DI
  KoreanLearningApp.Presentation/    ViewModel'и (без MAUI)
  KoreanLearningApp.Services/        бизнес-логика и интерфейсы
  KoreanLearningApp.Infrastructure/  SQLite, репозитории, импорт/экспорт
  KoreanLearningApp.Domain/          модели
tests/
  KoreanLearningApp.Tests/           xUnit-тесты
```

Подробности об архитектуре — в [CLAUDE.md](CLAUDE.md).

## Сборка и запуск

Нужен .NET 10 SDK с workload `maui`.

```bash
dotnet build src/KoreanLearningApp/KoreanLearningApp.csproj -f net10.0-windows10.0.19041.0
dotnet run --project src/KoreanLearningApp/KoreanLearningApp.csproj -f net10.0-windows10.0.19041.0
```

Android (эмулятор или телефон с включённой отладкой по USB):

```bash
dotnet build src/KoreanLearningApp/KoreanLearningApp.csproj -f net10.0-android -t:Install
```

Путь к проекту должен состоять из латинских символов, иначе Android-сборка падает с ошибкой `APT2265`.

## Тесты

```bash
dotnet test tests/KoreanLearningApp.Tests
```

Тесты запускаются в GitHub Actions на каждый push и pull request в `development` и `main`.

## Данные и логи

- База: `%LOCALAPPDATA%\koreanApp\KoreanLearning.db` (на Android — в папке приложения).
- Логи действий и ошибок: `%LOCALAPPDATA%\koreanApp\logs\app-yyyyMMdd.log`.

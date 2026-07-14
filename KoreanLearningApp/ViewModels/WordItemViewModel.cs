using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.ViewModels;

// Не путать с Domain.Word — это "презентационная" модель.
// Она хранит ссылку на реальное слово + добавляет UI-специфичную логику
// (например, состояние "избранное" и команду на его переключение),
// не загрязняя этим доменную модель.
public class WordItemViewModel : BaseViewModel
{
    public Word Word { get; }

    public WordItemViewModel(Word word)
    {
        Word = word;
    }

    public string Korean => Word.Korean;
    public string TranslationRu => Word.TranslationRu;
    public string Category => Word.Category;
}
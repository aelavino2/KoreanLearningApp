// ViewModels/BaseViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KoreanLearningApp.ViewModels;

// Базовый класс для всех ViewModel — реализует INotifyPropertyChanged,
// чтобы UI (XAML-биндинги) узнавал об изменении свойств.
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // Вызывать вручную, если нужно уведомить об изменении конкретного свойства
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Универсальный сеттер: сам обновляет поле и шлёт уведомление, если значение реально изменилось.
    // Использование: set { SetProperty(ref _field, value); }
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
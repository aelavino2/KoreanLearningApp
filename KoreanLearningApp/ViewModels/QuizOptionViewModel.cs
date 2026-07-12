using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
using KoreanLearningApp.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace KoreanLearningApp.ViewModels;

/// <summary>Один из четырёх вариантов ответа в обычном режиме.</summary>
public class QuizOptionViewModel : INotifyPropertyChanged
{
    private string _text = string.Empty;
    private bool _isVisible;
    private bool _isCorrectAnswer;
    private bool _isWrongAnswer;

    public string Text
    {
        get => _text;
        set { _text = value; OnPropertyChanged(); }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set { _isVisible = value; OnPropertyChanged(); }
    }

    public bool IsCorrectAnswer
    {
        get => _isCorrectAnswer;
        set { _isCorrectAnswer = value; OnPropertyChanged(); }
    }

    public bool IsWrongAnswer
    {
        get => _isWrongAnswer;
        set { _isWrongAnswer = value; OnPropertyChanged(); }
    }

    public void Reset()
    {
        IsCorrectAnswer = false;
        IsWrongAnswer = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
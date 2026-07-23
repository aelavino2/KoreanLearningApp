using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.ViewModels;

public partial class SenseItemViewModel : ObservableObject
{
    public Sense Sense { get; }

    public SenseItemViewModel(Sense sense)
    {
        Sense = sense;
    }

    public string EnText => Sense.EnWord;
    public string EnDefinition => Sense.EnDefinition;
    public string RuText => Sense.RuWord;
    public string RuDefinition => Sense.RuDefinition;

    [ObservableProperty]
    private bool _isExpanded;

    [RelayCommand]
    private void ToggleExpanded() => IsExpanded = !IsExpanded;
}
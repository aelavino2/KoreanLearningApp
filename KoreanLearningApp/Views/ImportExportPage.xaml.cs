using KoreanLearningApp.Infrastructure.Persistence;
using KoreanLearningApp.Services;
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class ImportExportPage : ContentPage
{
    private readonly ImportExportPageViewModel _viewModel;

    public ImportExportPage(DatabaseConstants db)
    {
        InitializeComponent();
        _viewModel = new ImportExportPageViewModel(db);
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshBackupPath();
    }
}
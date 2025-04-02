namespace Ui.ViewModels.Pages;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty] private int _counter;

    [RelayCommand]
    private void OnCounterIncrement()
    {
        Counter++;
    }
}
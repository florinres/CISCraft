using System.Windows.Media;
using Ui.Models;
using Wpf.Ui.Abstractions.Controls;

namespace Ui.ViewModels.Pages;

public partial class DataViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty] private IEnumerable<DataColor> _colors;

    private bool _isInitialized;

    public Task OnNavigatedToAsync()
    {
        if (!_isInitialized)
            InitializeViewModel();

        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

    private void InitializeViewModel()
    {
        var random = new Random();
        var colorCollection = new List<DataColor>();

        for (var i = 0; i < 8192; i++)
            colorCollection.Add(
                new DataColor
                {
                    Color = new SolidColorBrush(
                        Color.FromArgb(
                            200,
                            (byte)random.Next(0, 250),
                            (byte)random.Next(0, 250),
                            (byte)random.Next(0, 250)
                        )
                    )
                }
            );

        Colors = colorCollection;

        _isInitialized = true;
    }
}
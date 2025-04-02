using Ui.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace Ui.Views.Pages;

public partial class SettingsPage : INavigableView<SettingsViewModel>
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public SettingsViewModel ViewModel { get; }
}
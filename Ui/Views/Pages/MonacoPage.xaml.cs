using Ui.ViewModels.Pages.Monaco;
using Wpf.Ui.Abstractions.Controls;

namespace Ui.Views.Pages;

public partial class MonacoPage : INavigableView<IMonacoViewModel>
{
    public MonacoPage(IMonacoViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
        ViewModel.SetWebView(WebView);
    }

    public IMonacoViewModel ViewModel { get; }
}
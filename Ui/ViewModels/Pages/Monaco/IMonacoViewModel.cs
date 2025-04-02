using Microsoft.Web.WebView2.Wpf;

namespace Ui.ViewModels.Pages.Monaco;

public interface IMonacoViewModel : IViewModel
{
    /// <summary>
    ///     Gets an <see cref="global::CommunityToolkit.Mvvm.Input.IAsyncRelayCommand{T}" /> instance wrapping
    ///     <see cref="MonacoViewModel.OnMenuAction" />.
    /// </summary>
    IAsyncRelayCommand<string> MenuActionCommand { get; }

    void SetWebView(WebView2 webView);
    Task OnMenuAction(string parameter);
}
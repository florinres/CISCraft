// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.IO;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Wpf;
using Ui.Controllers;
using Ui.Models.Monaco;
using Wpf.Ui.Gallery.ViewModels;

namespace Ui.ViewModels.Pages;

public partial class MonacoViewModel : ViewModel
{
    private MonacoController _monacoController;
    private ILogger<MonacoViewModel> _logger;

    public MonacoViewModel(MonacoController monacoController, ILogger<MonacoViewModel> logger)
    {
        _monacoController = monacoController;
        _logger = logger;
    }

    public void SetWebView(WebView2 webView)
    {
        webView.NavigationCompleted += OnWebViewNavigationCompleted;
        webView.SetCurrentValue(FrameworkElement.UseLayoutRoundingProperty, true);
        webView.SetCurrentValue(WebView2.DefaultBackgroundColorProperty, System.Drawing.Color.Transparent);
        webView.SetCurrentValue(
            WebView2.SourceProperty,
            new Uri(
                System.IO.Path.Combine(
                    System.AppDomain.CurrentDomain.BaseDirectory,
                    @"Assets\Monaco\index.html"
                )
            )
        );

        _monacoController = new MonacoController(webView);
    }

    [RelayCommand]
    public async Task OnMenuAction(string parameter)
    {
        if (parameter == "openFile")
        {
             await OpenFileAsync();
        }
        else if (parameter == "saveFile")
        {
            _logger.LogInformation(await _monacoController.GetContentAsync());
        }
    }
    
    private async Task OpenFileAsync()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Asm files (*.asm)|*.asm|All files (*.*)|*.*",
            Title = "Open a File"
        };

        bool? result = openFileDialog.ShowDialog();
    
        if (result == true)
        {
            string filePath = openFileDialog.FileName;

            // Now move the file reading operation to a background thread
            string fileContent = await File.ReadAllTextAsync(filePath);
        
            // After reading the file, update Monaco on the UI thread
            await _monacoController!.SetContentAsync(fileContent);
        }

    }

    private async Task InitializeEditorAsync()
    {
        await _monacoController.CreateAsync();
        await _monacoController.SetThemeAsync(Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme());
        await _monacoController.SetLanguageAsync(MonacoLanguage.R);
        // await _monacoController.SetContentAsync(
        //     "// This Source Code Form is subject to the terms of the MIT License.\r\n// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.\r\n// Copyright (C) Leszek Pomianowski and WPF UI Contributors.\r\n// All Rights Reserved.\r\n\r\nnamespace Wpf.Ui.Gallery.Models.Monaco;\r\n\r\n[Serializable]\r\npublic record MonacoTheme\r\n{\r\n    public string Base { get; init; }\r\n\r\n    public bool Inherit { get; init; }\r\n\r\n    public IDictionary<string, string> Rules { get; init; }\r\n\r\n    public IDictionary<string, string> Colors { get; init; }\r\n}\r\n"
        // );
    }

    private void OnWebViewNavigationCompleted(
        object? sender,
        Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e
    )
    {
        _ = DispatchAsync(InitializeEditorAsync);
    }

    private static DispatcherOperation<TResult> DispatchAsync<TResult>(Func<TResult> callback)
    {
        return Application.Current.Dispatcher.InvokeAsync(callback);
    }
}

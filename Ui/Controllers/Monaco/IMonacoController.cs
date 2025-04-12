using Ui.Models.Monaco;
using Wpf.Ui.Appearance;

namespace Ui.Controllers.Monaco;

public interface IMonacoController
{
    public Uri AssetsPath { get; }
    Task CreateAsync();
    Task SetThemeAsync(ApplicationTheme appApplicationTheme);
    Task SetLanguageAsync(MonacoLanguage monacoLanguage);
    Task SetContentAsync(string contents);
    Task<string> GetContentAsync();
    void DispatchScript(string script);
}
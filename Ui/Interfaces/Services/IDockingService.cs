using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.Services;

public interface IDockingService
{
    void ShowTool(IToolViewModel tool);
    void ToggleVisibility(IToolViewModel tool);
    void SaveLayout(string fileName);
    void LoadLayout(string fileName);
    List<string> GetAllLayoutNames();
    void DeleteLayout(string fileName);
    void SaveLastUsedLayout();
    void LoadLastUsedLayout();
    
    /// <summary>
    /// Restores the default layout by clearing any saved layout state and resetting all tools to their default configuration.
    /// This method is useful for recovering from corrupted layouts that may cause the application to crash.
    /// </summary>
    void RestoreDefaultLayout();
}
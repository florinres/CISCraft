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
    void RestoreDefaultLayout();
}
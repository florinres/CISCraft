using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.Services;

public interface IDockingService
{
    void ShowTool(IToolViewModel tool);
    void ToggleVisibility(IToolViewModel tool);
    void SaveLayout(string filePath);
    void LoadLayout(string filePath);
}
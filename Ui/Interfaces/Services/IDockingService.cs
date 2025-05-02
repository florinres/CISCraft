using Ui.ViewModels.Generics;

namespace Ui.Services;

public interface IDockingService
{
    void ShowTool(ToolViewModel tool);
    void ToggleVisibility(ToolViewModel tool);
}
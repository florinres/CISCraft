using Ui.ViewModels.Generics;

namespace Ui.Services;

public interface IToolVisibilityService
{
    void ToggleToolVisibility(ToolViewModel? tool);
    void SetDockingService(IDockingService dockingService);
}
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.Services;

public interface IToolVisibilityService
{
    void ToggleToolVisibility(ToolViewModel? tool);
    void SetDockingService(IDockingService dockingService);
}
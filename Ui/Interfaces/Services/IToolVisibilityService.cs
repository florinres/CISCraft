using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.Services;

public interface IToolVisibilityService
{
    void ToggleToolVisibility(IToolViewModel? tool);
    void SetDockingService(IDockingService dockingService);
}
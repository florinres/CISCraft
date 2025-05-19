using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.Services;

public class ToolVisibilityService : IToolVisibilityService
{
    private IDockingService _dockingService;

    public ToolVisibilityService(IDockingService dockingService)
    {
        _dockingService = dockingService;
    }

    public void ToggleToolVisibility(IToolViewModel? tool)
    {
        if (tool == null)
            return;

        if (tool.HasToolBeenLoaded == false)
        {
            _dockingService.ShowTool(tool);
            tool.HasToolBeenLoaded = true;
        }
        else
        {
            _dockingService.ToggleVisibility(tool);
        }
    }

    public void SetDockingService(IDockingService dockingService)
    {
        _dockingService = dockingService;
    }
}
using Ui.ViewModels.Generics;

namespace Ui.Services;

public class DummyDockingService : IDockingService
{
    public void ShowTool(ToolViewModel tool)
    {
        throw new NotImplementedException();
    }

    public void ToggleVisibility(ToolViewModel tool)
    {
        throw new NotImplementedException();
    }
}
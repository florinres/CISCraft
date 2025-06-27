using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.Services;

public class DummyDockingService : IDockingService
{
    public void ShowTool(IToolViewModel tool)
    {
        throw new NotImplementedException();
    }

    public void ToggleVisibility(IToolViewModel tool)
    {
        throw new NotImplementedException();
    }

    public void SaveLayout(string filePath)
    {
        throw new NotImplementedException();
    }

    public void LoadLayout(string filePath)
    {
        throw new NotImplementedException();
    }
}
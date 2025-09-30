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

    public void SaveLayout(string fileName)
    {
        throw new NotImplementedException();
    }

    public void LoadLayout(string fileName)
    {
        throw new NotImplementedException();
    }

    public List<string> GetAllLayoutNames()
    {
        throw new NotImplementedException();
    }

    public void DeleteLayout(string fileName)
    {
        throw new NotImplementedException();
    }

    public void SaveLastUsedLayout()
    {
        throw new NotImplementedException();
    }
}
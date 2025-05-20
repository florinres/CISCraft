using System.Text;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Services;

namespace Ui.ViewModels.Generics;

public partial class ActionsBarViewModel : ObservableObject, IActionsBarViewModel
{
    private readonly IActiveDocumentService _activeDocumentService;
    private readonly IAssemblerService _assemblerService;
    private readonly CpuService _cpuService;

    public ActionsBarViewModel(IAssemblerService assemblerService, IActiveDocumentService activeDocumentService, CpuService cpuService)
    {
        _assemblerService = assemblerService;
        _activeDocumentService = activeDocumentService;
        _cpuService = cpuService;

        ObjectCodeGenerated += OnObjectCodeGenerated;
    }

    public event EventHandler<byte[]>? ObjectCodeGenerated;

    [RelayCommand]
    private async Task RunAssembleSourceCodeService()
    {
        if (_activeDocumentService.SelectedDocument == null) return;
        var objectCode = await Task.Run(() => _assemblerService.AssembleSourceCodeService(_activeDocumentService.SelectedDocument.Content));
        ObjectCodeGenerated?.Invoke(this, objectCode);
    }
    
    [RelayCommand]
    private async Task LoadJson()
    {
        await Task.Run(() => _cpuService.LoadJsonMpm());
    }
    
    [RelayCommand]
    private void StepMicroprogram()
    {
        _cpuService.StepMicrocode();
    }

    private void OnObjectCodeGenerated(object? sender, byte[] objectCode)
    {
        if (objectCode is not [])
        {
            _activeDocumentService.HexViewer.IsVisible = true;
        }
    }
}
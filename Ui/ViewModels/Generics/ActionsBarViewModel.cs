using System.Text;
using Microsoft.Win32;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Services;

namespace Ui.ViewModels.Generics;

public partial class ActionsBarViewModel : ObservableObject, IActionsBarViewModel
{
    private readonly IActiveDocumentService _activeDocumentService;
    private readonly IAssemblerService _assemblerService;
    private readonly ICpuService _cpuService;
    private readonly IToolVisibilityService _toolVisibilityService;

    [ObservableProperty]
    public partial bool IsDebugging { get; set; }
    [ObservableProperty]
    public partial bool NotDebugging { get; set; }

    public ActionsBarViewModel(IAssemblerService assemblerService, IActiveDocumentService activeDocumentService, ICpuService cpuService, IToolVisibilityService toolVisibilityService)
    {
        _assemblerService = assemblerService;
        _activeDocumentService = activeDocumentService;
        _cpuService = cpuService;
        _toolVisibilityService = toolVisibilityService;
        IsDebugging = false;
        NotDebugging = true;
        ObjectCodeGenerated += OnObjectCodeGenerated;
    }

    public event EventHandler<byte[]>? ObjectCodeGenerated;

    [RelayCommand]
    private async Task RunAssembleSourceCodeService()
    {
        if (_activeDocumentService.SelectedDocument == null) return;
        var objectCode = await Task.Run(() => _assemblerService.AssembleSourceCodeService(_activeDocumentService.SelectedDocument.Content));
        ObjectCodeGenerated?.Invoke(this, objectCode);
        
        _toolVisibilityService.ToggleToolVisibility(_activeDocumentService.HexViewer);
    }
    
    [RelayCommand]
    private async Task LoadJson()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open File",
            Filter = "MPM File (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true) return;
        
        await Task.Run(
            
            () => _cpuService.LoadJsonMpm(dialog.FileName)
            
            );
    }
    [RelayCommand]
    private void StepMicroprogram()
    {
        _cpuService.StepMicrocommand();
    }
    [RelayCommand]
    private void StartDebug()
    {
        if (_activeDocumentService.SelectedDocument != null)
        {
            _cpuService.StartDebugging();
            IsDebugging = true;
            NotDebugging = false;
        }
    }
    [RelayCommand]
    private void StopDebug()
    {
        if (_activeDocumentService.SelectedDocument != null)
        {
            _cpuService.StopDebugging();
            IsDebugging = false;
            NotDebugging = true;
        }
    }
    [RelayCommand]
    private void ResetProgram()
    {
        _cpuService.ResetProgram();
    }
    private void OnObjectCodeGenerated(object? sender, byte[] objectCode)
    {
        if (objectCode is not [])
        {
            _activeDocumentService.HexViewer.IsVisible = true;
        }
    }
}
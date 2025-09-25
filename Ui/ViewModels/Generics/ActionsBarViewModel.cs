using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Models;
using Ui.Services;
using Ui.ViewModels.Components.MenuBar;
using Ui.Views.Windows;


namespace Ui.ViewModels.Generics;

public partial class ActionsBarViewModel : ObservableObject, IActionsBarViewModel
{
    private readonly IActiveDocumentService _activeDocumentService;
    private readonly IAssemblerService _assemblerService;
    private readonly ICpuService _cpuService;
    private readonly IToolVisibilityService _toolVisibilityService;
    const ushort USER_CODE_START_ADDR = 0x0010;
    [ObservableProperty]
    public partial bool IsDebugging { get; set; }
    [ObservableProperty]
    public partial bool NotDebugging { get; set; }
    [ObservableProperty]
    public partial bool CanDebug { get; set; }
    [ObservableProperty]
    public partial bool CanAssemble{ get; set; }
    [ObservableProperty]
    public partial StepLevel StepLevel { get; set; }
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    public ActionsBarViewModel(IAssemblerService assemblerService, IActiveDocumentService activeDocumentService, ICpuService cpuService, IToolVisibilityService toolVisibilityService)
    {
        _assemblerService = assemblerService;
        _activeDocumentService = activeDocumentService;
        _cpuService = cpuService;
        _toolVisibilityService = toolVisibilityService;
        IsDebugging = false;
        NotDebugging = true;
        CanDebug = false;
        CanAssemble = false;
        StepLevel = StepLevel.Microcommand;
    }
    [RelayCommand]
    public void SetStepLevel(StepLevel level)
    {
        StepLevel = level;
    }

    [RelayCommand]
    private void RunCode()
    {
        ushort haltCode = 0xE300;
        ushort irValue = _cpuService.GetIR();
        while (irValue != haltCode)
        {
            _cpuService.StepMicroinstruction();
            irValue = _cpuService.GetIR();
        }
        _cpuService.StepMicroinstruction();
        _cpuService.StepMicroinstruction();
        _cpuService.StepMicroinstruction();
        _cpuService.StepMicroinstruction();
        _cpuService.StepMicroinstruction();
        CanAssemble = true;
        IsDebugging = true;
        NotDebugging = false;
        CanDebug = false;
    }

    [RelayCommand]
    private async Task RunAssembleSourceCodeService()
    {
        if (_activeDocumentService.SelectedDocument == null) return;
        var debugSymbols = await Task.Run(() => _assemblerService.AssembleSourceCodeService(_activeDocumentService.SelectedDocument.Content, USER_CODE_START_ADDR));
        if (debugSymbols == null)
        {
            CanAssemble = true;
            CanDebug = false;
            return;
        }

        _cpuService.UpdateDebugSymbols(_activeDocumentService.SelectedDocument.Content, debugSymbols, USER_CODE_START_ADDR);
        
        _toolVisibilityService.ToggleToolVisibility(_activeDocumentService.HexViewer);
        CanDebug = true;
        CanAssemble = false;
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
    private void Step()
    {
        switch (StepLevel)
        {
            case StepLevel.Microcommand:
                _cpuService.StepMicrocommand();
                break;
            case StepLevel.Microinstruction:
                _cpuService.StepMicroinstruction();
                break;
            case StepLevel.Instruction:
                _cpuService.StepInstruction();
                break;
        }
    }
    [RelayCommand]
    private void StartDebug()
    {
        if (_activeDocumentService.SelectedDocument != null)
        {
            _cpuService.StartDebugging();
            IsDebugging = true;
            NotDebugging = false;
            CanDebug = false;
            CanAssemble = false;
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
            CanDebug = true;
            CanAssemble = true;
        }
    }
    [RelayCommand]
    private void ResetProgram()
    {
        _cpuService.ResetProgram();
    }
    [RelayCommand]
    public void TriggerInterrupt(ISR isr)
    {
        _cpuService.TriggerInterrupt(isr);
    }

    [RelayCommand]
    private void OpenReferenceManual()
    {
        var pdfPath = Path.GetFullPath(AppContext.BaseDirectory + "/../../../../Docs/reference_manual.pdf");
        if (File.Exists(pdfPath))
            Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });
    }

    [RelayCommand]

    private void OpenDevGuide()
    {
        var pdfPath = Path.GetFullPath(AppContext.BaseDirectory + "/../../../../Docs/dev_guide.pdf");
        if (File.Exists(pdfPath))
            Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });
    }

    [RelayCommand]
    private void OpenISA()
    {
        var pdfPath = Path.GetFullPath(AppContext.BaseDirectory + "/../../../../Docs/isa.pdf");
        if (File.Exists(pdfPath))
            Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });
    }

    [RelayCommand]
    private void OpenInstructionEncoding()
    {
        var pdfPath = Path.GetFullPath(AppContext.BaseDirectory + "/../../../../Docs/instruction_encoding.pdf");
        if (File.Exists(pdfPath))
            Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });
    }
}
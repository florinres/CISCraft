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
    [ObservableProperty]
    public partial bool IsDebugging { get; set; }
    [ObservableProperty]
    public partial bool CanDebug { get; set; }
    [ObservableProperty]
    public partial bool CanAssemble{ get; set; }
    [ObservableProperty]
    public partial bool IsInterruptSaveButtonVisible { get; set; }
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
        CanDebug = false;
        CanAssemble = false;
        StepLevel = StepLevel.Microcommand;
        ObjectCodeGenerated += OnObjectCodeGenerated;
    }

    public event EventHandler<byte[]>? ObjectCodeGenerated;

    [RelayCommand]
    public void SetStepLevel(StepLevel level)
    {
        StepLevel = level;
    }
    [RelayCommand]
    private async Task RunAssembleSourceCodeService()
    {
        if (_activeDocumentService.SelectedDocument == null) return;
        var objectCode = await Task.Run(() => _assemblerService.AssembleSourceCodeService(_activeDocumentService.SelectedDocument.Content));
        ObjectCodeGenerated?.Invoke(this, objectCode);
        
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
            _cpuService.SetDebugSymbols(_assemblerService.DebugSymbols);
            _cpuService.StartDebugging();
            IsDebugging = true;
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
    private void SaveInterrupt(FileViewModel activeFile)
    {
        if (MainWindow.Isrs is null) return;

        foreach(var isr in MainWindow.Isrs)
        {
            if (isr.Name == activeFile.Title)
            {
                isr.TextCode = activeFile.Content;
                WriteIsrsToJson(MainWindow.Isrs);
                IsInterruptSaveButtonVisible = false;
                break;
            }
        }

        MenuBarViewModel.files.Remove(activeFile);
    }
    private void OnObjectCodeGenerated(object? sender, byte[] objectCode)
    {
        if (objectCode is not [])
        {
            _activeDocumentService.HexViewer.IsVisible = true;
        }
    }

    private void WriteIsrsToJson(List<ISR> isrs)
    {
        string currentFolder = Path.GetFullPath(AppContext.BaseDirectory + "../../../../");
        string jsonPath = Path.Combine(currentFolder + "Configs", "IVT.json");
        var json = JsonSerializer.Serialize(isrs, JsonOpts);
        File.WriteAllText(jsonPath, json);
    }
}
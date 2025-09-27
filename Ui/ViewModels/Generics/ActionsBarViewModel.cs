using Assembler.Business;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Threading;
using Ui.Components;
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
    private ErrorMarkerService? _errorMarkerService;
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
    public partial bool CanRun { get; set; }
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
        
        // Subscribe to assembly errors event
        _assemblerService.AssemblyErrorsReported += OnAssemblyErrorsReported;
        
        // Listen for document changes to reinitialize the error marker service
        _activeDocumentService.ActiveDocumentChanged += (sender, e) => InitializeErrorMarkerService();
    }
    
    /// <summary>
    /// Initializes or reinitializes the error marker service for the current document
    /// </summary>
    private void InitializeErrorMarkerService()
    {
        // Ensure this is called on the UI thread
        if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
        {
            System.Windows.Application.Current.Dispatcher.Invoke(InitializeErrorMarkerService);
            return;
        }
        
        // Clean up existing service
        _errorMarkerService?.Dispose();
        _errorMarkerService = null;
        
        // Check if there's an active document with a StyledAvalonEdit
        if (_activeDocumentService.SelectedDocument?.Editor is StyledAvalonEdit editor)
        {
            _errorMarkerService = new ErrorMarkerService(editor);
        }
    }
    
    /// <summary>
    /// Handles reported assembly errors and displays them in the editor
    /// </summary>
    private void OnAssemblyErrorsReported(object? sender, IReadOnlyList<AssemblyError> errors)
    {
        // Use the dispatcher to ensure UI updates happen on the UI thread
        // Get the dispatcher from Application.Current.Dispatcher or from a UI element
        Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;
        
        // Use BeginInvoke to run the UI update code on the UI thread
        dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                // Make sure the error marker service is initialized
                InitializeErrorMarkerService();
                
                // Clear existing markers
                _errorMarkerService?.Clear();
                
                // Add new error markers
                foreach (var error in errors)
                {
                    try
                    {
                        // Calculate end position (if not provided by the assembler)
                        int endLine = error.Line;
                        int endColumn = error.Column + Math.Max(1, error.Length);
                        
                        _errorMarkerService?.Create(
                            error.Line - 1, // Adjust to 0-based for editor display
                            error.Column - 1, // Adjust to 0-based for editor display
                            endLine - 1, // Adjust to 0-based for editor display
                            endColumn - 1, // Adjust to 0-based for editor display
                            error.Message,
                            ErrorType.Error);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to create error marker: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing assembly errors on UI thread: {ex.Message}");
            }
        }));
    }
    [RelayCommand]
    public void SetStepLevel(StepLevel level)
    {
        StepLevel = level;
    }
    [RelayCommand]
    private async Task RunCode()
    {
        if (_activeDocumentService.SelectedDocument == null) return;

        ushort haltCode = 0xE300;
        ushort irValue = _cpuService.GetIR();

        while (irValue != haltCode)
        {

            _cpuService.StepInstruction();
            irValue = _cpuService.GetIR();

            await Task.Yield();
            await Task.Delay(1);
        }

        for (int i = 0; i < 5; i++)
        {
            _cpuService.StepMicroinstruction();
        }

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
            CanRun = false;
            
            // Errors are already processed through the AssemblyErrorsReported event
            return;
        }
        
        // Clear any error markers since assembly succeeded - ensure on UI thread
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => {
            _errorMarkerService?.Clear();
        }));

        _cpuService.UpdateDebugSymbols(_activeDocumentService.SelectedDocument.Content, debugSymbols, USER_CODE_START_ADDR);

        _toolVisibilityService.ToggleToolVisibility(_activeDocumentService.HexViewer);
        CanDebug = true;
        CanAssemble = false;
        CanRun = true;
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
    
    /// <summary>
    /// Clears any error markers in the active document editor
    /// </summary>
    public void ClearErrorMarkers()
    {
        // Ensure we're on the UI thread
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => {
            _errorMarkerService?.Clear();
        }));
    }
}
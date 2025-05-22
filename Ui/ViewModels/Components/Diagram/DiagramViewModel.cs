using System.ComponentModel;
using CPU.Business.Models;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Components.Diagram;

public partial class DiagramViewModel : ToolViewModel, IDiagramViewModel
{
    private readonly RegisterWrapper _registers;

    public DiagramViewModel(IMicroprogramViewModel microprogramViewModel, RegisterWrapper registers)
    {
        MemoryContext = microprogramViewModel;
        _registers = registers;
        BindToRegisters();
        SetUpContext();
    }
    
    
    
    [ObservableProperty] public override partial string? Title { get; set; } = "Diagram";

    #region Contexts
    public IMicroprogramViewModel MemoryContext { get; set; }
    public RegisterViewModel DataInContext   { get; } = new("DATA IN");
    public RegisterViewModel DataOutContext  { get; } = new("DATA OUT");
    public RegisterViewModel PCContext       { get; } = new("PC");
    public RegisterViewModel IVRContext      { get; } = new("IVR");
    public RegisterViewModel TContext        { get; } = new("T");
    public RegisterViewModel SPContext       { get; } = new("SP");
    public RegisterViewModel FLAGContext     { get; } = new("FLAG");
    public RegisterViewModel MDRContext      { get; } = new("MDR");
    public RegisterViewModel ADRContext      { get; } = new("ADR");
    public RegisterViewModel IRContext       { get; } = new("IR");
    public RegisterViewModel R0Context       { get; } = new("R0");
    public RegisterViewModel R1Context       { get; } = new("R1");
    public RegisterViewModel R2Context       { get; } = new("R2");
    public RegisterViewModel R3Context       { get; } = new("R3");
    public RegisterViewModel R4Context       { get; } = new("R4");
    public RegisterViewModel R5Context       { get; } = new("R5");
    public RegisterViewModel R6Context       { get; } = new("R6");
    public RegisterViewModel R7Context       { get; } = new("R7");
    public RegisterViewModel R8Context       { get; } = new("R8");
    public RegisterViewModel R9Context       { get; } = new("R9");
    public RegisterViewModel R10Context      { get; } = new("R10");
    public RegisterViewModel R11Context      { get; } = new("R11");
    public RegisterViewModel R12Context      { get; } = new("R12");
    public RegisterViewModel R13Context      { get; } = new("R13");
    public RegisterViewModel R14Context      { get; } = new("R14");
    public RegisterViewModel R15Context      { get; } = new("R15");
    
    public BitBlockViewModel Pd1Context      { get; } = new("Pd1");
    public BitBlockViewModel PdMinus1Context { get; } = new("Pd-1");
    public BitBlockViewModel Pd0sContext     { get; } = new("Pd0s");
    #endregion

    public List<BaseDiagramObject> Contexts { get; set; } = [];

    public void ResetHighlight()
    {
        foreach (var context in Contexts)
        {
            context.IsHighlighted = false;
        }
    }

    private void SetUpContext()
    {
        Contexts =
        [
            DataInContext,
            DataOutContext,
            PCContext,
            IVRContext,
            TContext,
            SPContext,
            FLAGContext,
            MDRContext,
            ADRContext,
            IRContext,
            R0Context,
            R1Context,
            R2Context,
            R3Context,
            R4Context,
            R5Context,
            R6Context,
            R7Context,
            R8Context,
            R9Context,
            R10Context,
            R11Context,
            R12Context,
            R13Context,
            R14Context,
            R15Context,
            Pd1Context,
            PdMinus1Context,
            Pd0sContext
        ];
        ResetHighlight();
    }
    
    public void BindToRegisters()
    {
        _registers.PropertyChanged += OnRegistersChanged;

        foreach (REGISTERS reg in Enum.GetValues<REGISTERS>())
            UpdateRegister(reg, _registers[reg]);
        
        foreach (GPR gpr in Enum.GetValues<GPR>())
            UpdateGpr(gpr, _registers[gpr]);
    }
    
    private void OnRegistersChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName?.StartsWith("Registers[") == true)
        {
            var name = e.PropertyName[10..^1]; // get name inside brackets
            if (Enum.TryParse<REGISTERS>(name, out var reg))
                UpdateRegister(reg, _registers[reg]);
        }
        else if (e.PropertyName?.StartsWith("Gpr[") == true)
        {
            var name = e.PropertyName[4..^1];
            if (Enum.TryParse<GPR>(name, out var gpr))
                UpdateGpr(gpr, _registers[gpr]);
        }
    }

    private BaseDiagramObject? _lastUpdatedObject;
    
    private void UpdateRegister(REGISTERS reg, short value)
    {
        switch (reg)
        {
            case REGISTERS.FLAGS:
                FLAGContext.Value = value;
                FLAGContext.IsHighlighted = true;
                _lastUpdatedObject = FLAGContext;
                break;
            // case REGISTERS.RG:
            //     PCContext.Value = value;
            //     PCContext.IsHighlighted = true;
            //     LastUpdatedObject = PCContext;
            //     break;
            case REGISTERS.SP:
                SPContext.Value = value;
                SPContext.IsHighlighted = true;
                _lastUpdatedObject = SPContext;
                break;
            case REGISTERS.T:
                TContext.Value = value;
                TContext.IsHighlighted = true;
                _lastUpdatedObject = TContext;
                break;
            case REGISTERS.PC:
                PCContext.Value = value;
                PCContext.IsHighlighted = true;
                _lastUpdatedObject = PCContext;
                break;
            case REGISTERS.IVR:
                IVRContext.Value = value;
                IVRContext.IsHighlighted = true;
                _lastUpdatedObject = IVRContext;
                break;
            case REGISTERS.ADR:
                ADRContext.Value = value;
                ADRContext.IsHighlighted = true;
                _lastUpdatedObject = ADRContext;
                break;
            case REGISTERS.MDR:
                MDRContext.Value = value;
                MDRContext.IsHighlighted = true;
                _lastUpdatedObject = MDRContext;
                break;
            case REGISTERS.IR:
                IRContext.Value = value;
                IRContext.IsHighlighted = true;
                _lastUpdatedObject = IRContext;
                break;
            // Handle others as needed
        }
    }

    private void UpdateGpr(GPR gpr, short value)
    {
        switch (gpr)
        {
            case GPR.R0:
                R0Context.Value = value;
                R0Context.IsHighlighted = true;
                _lastUpdatedObject = R0Context;
                break;
            case GPR.R1:
                R1Context.Value = value;
                R1Context.IsHighlighted = true;
                _lastUpdatedObject = R1Context;
                break;
            case GPR.R2:
                R2Context.Value = value;
                R2Context.IsHighlighted = true;
                _lastUpdatedObject = R2Context;
                break;
            case GPR.R3:
                R3Context.Value = value;
                R3Context.IsHighlighted = true;
                _lastUpdatedObject = R3Context;
                break;
            case GPR.R4:
                R4Context.Value = value;
                R4Context.IsHighlighted = true;
                _lastUpdatedObject = R4Context;
                break;
            case GPR.R5:
                R5Context.Value = value;
                R5Context.IsHighlighted = true;
                _lastUpdatedObject = R5Context;
                break;
            case GPR.R6:
                R6Context.Value = value;
                R6Context.IsHighlighted = true;
                _lastUpdatedObject = R6Context;
                break;
            case GPR.R7:
                R7Context.Value = value;
                R7Context.IsHighlighted = true;
                _lastUpdatedObject = R7Context;
                break;
            case GPR.R8:
                R8Context.Value = value;
                R8Context.IsHighlighted = true;
                _lastUpdatedObject = R8Context;
                break;
            case GPR.R9:
                R9Context.Value = value;
                R9Context.IsHighlighted = true;
                _lastUpdatedObject = R9Context;
                break;
            case GPR.R10:
                R10Context.Value = value;
                R10Context.IsHighlighted = true;
                _lastUpdatedObject = R10Context;
                break;
            case GPR.R11:
                R11Context.Value = value;
                R11Context.IsHighlighted = true;
                _lastUpdatedObject = R11Context;
                break;
            case GPR.R12:
                R12Context.Value = value;
                R12Context.IsHighlighted = true;
                _lastUpdatedObject = R12Context;
                break;
            case GPR.R13:
                R13Context.Value = value;
                R13Context.IsHighlighted = true;
                _lastUpdatedObject = R13Context;
                break;
            case GPR.R14:
                R14Context.Value = value;
                R14Context.IsHighlighted = true;
                _lastUpdatedObject = R14Context;
                break;
            case GPR.R15:
                R15Context.Value = value;
                R15Context.IsHighlighted = true;
                _lastUpdatedObject = R15Context;
                break;
        }
    }


}
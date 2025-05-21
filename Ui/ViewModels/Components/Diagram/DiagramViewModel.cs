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
    }
    
    
    
    [ObservableProperty] public override partial string? Title { get; set; } = "Diagram";

    #region Contexts
    public IMicroprogramViewModel MemoryContext { get; set; }
    public RegisterViewModel DataInContext { get; } = new("DATA IN");
    public RegisterViewModel DataOutContext { get; } = new("DATA OUT");
    public RegisterViewModel PCContext { get; } = new("PC");
    public RegisterViewModel IVRContext { get; } = new("IVR");
    public RegisterViewModel TContext { get; } = new("T");
    public RegisterViewModel SPContext { get; } = new("SP");
    public RegisterViewModel FLAGContext { get; } = new("FLAG");
    public RegisterViewModel MDRContext { get; } = new("MDR");
    public RegisterViewModel ADRContext { get; } = new("ADR");
    public RegisterViewModel IRContext { get; } = new("IR");
    public RegisterViewModel R0Context  { get; } = new("R0");
    public RegisterViewModel R1Context  { get; } = new("R1");
    public RegisterViewModel R2Context  { get; } = new("R2");
    public RegisterViewModel R3Context  { get; } = new("R3");
    public RegisterViewModel R4Context  { get; } = new("R4");
    public RegisterViewModel R5Context  { get; } = new("R5");
    public RegisterViewModel R6Context  { get; } = new("R6");
    public RegisterViewModel R7Context  { get; } = new("R7");
    public RegisterViewModel R8Context  { get; } = new("R8");
    public RegisterViewModel R9Context  { get; } = new("R9");
    public RegisterViewModel R10Context { get; } = new("R10");
    public RegisterViewModel R11Context { get; } = new("R11");
    public RegisterViewModel R12Context { get; } = new("R12");
    public RegisterViewModel R13Context { get; } = new("R13");
    public RegisterViewModel R14Context { get; } = new("R14");
    public RegisterViewModel R15Context { get; } = new("R15");
    
    public BitBlockViewModel Pd1Context { get; } = new("Pd1");
    public BitBlockViewModel PdMinus1Context { get; } = new("Pd-1");
    public BitBlockViewModel Pd0sContext { get; } = new("Pd0s");
    #endregion
    
    public void BindToRegisters()
    {
        _registers.PropertyChanged += OnRegistersChanged;

        // Initialize values
        // foreach (REGISTERS reg in Enum.GetValues<REGISTERS>())
        //     UpdateRegister(reg, _registers[reg]);
        //
        // foreach (GPR gpr in Enum.GetValues<GPR>())
        //     UpdateGpr(gpr, _registers[gpr]);
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

    private void UpdateRegister(REGISTERS reg, short value)
    {
        switch (reg)
        {
            case REGISTERS.FLAGS:
                FLAGContext.Value = value;
                break;
            case REGISTERS.RG:
                PCContext.Value = value;
                break;
            case REGISTERS.SP:
                SPContext.Value = value;
                break;
            case REGISTERS.T:
                TContext.Value = value;
                break;
            case REGISTERS.PC:
                PCContext.Value = value;
                break;
            case REGISTERS.IVR:
                IVRContext.Value = value;
                break;
            case REGISTERS.ADR:
                ADRContext.Value = value;
                break;
            case REGISTERS.MDR:
                MDRContext.Value = value;
                break;
            case REGISTERS.IR:
                IRContext.Value = value;
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
                break;
            case GPR.R1:
                R1Context.Value = value;
                break;
            case GPR.R2:
                R2Context.Value = value;
                break;
            case GPR.R3:
                R3Context.Value = value;
                break;
            case GPR.R4:
                R4Context.Value = value;
                break;
            case GPR.R5:
                R5Context.Value = value;
                break;
            case GPR.R6:
                R6Context.Value = value;
                break;
            case GPR.R7:
                R7Context.Value = value;
                break;
            case GPR.R8:
                R8Context.Value = value;
                break;
            case GPR.R9:
                R9Context.Value = value;
                break;
            case GPR.R10:
                R10Context.Value = value;
                break;
            case GPR.R11:
                R11Context.Value = value;
                break;
            case GPR.R12:
                R12Context.Value = value;
                break;
            case GPR.R13:
                R13Context.Value = value;
                break;
            case GPR.R14:
                R14Context.Value = value;
                break;
            case GPR.R15:
                R15Context.Value = value;
                break;
        }
    }


}
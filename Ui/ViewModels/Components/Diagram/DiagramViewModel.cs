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
    public RegisterViewModel MirContext      { get; } = new("MIR");
    public RegisterViewModel MarContext      { get; } = new("MAR");
    
    public BitBlockViewModel BVIContext      { get; } = new("BVI");
    public BitBlockViewModel CContext        { get; } = new("C");
    public BitBlockViewModel ZContext        { get; } = new("Z");
    public BitBlockViewModel SContext        { get; } = new("S");
    public BitBlockViewModel VContext        { get; } = new("V");
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
        // This could be done through some sort of discovery or annotations, but I don't see the point.
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
            BVIContext,
            CContext,
            ZContext,
            SContext,
            VContext,
            MirContext,
            MarContext
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
        
        Update(MirContext, _registers.MIR);
        Update(MarContext, _registers.MAR);
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
        else if (e.PropertyName?.StartsWith("MIR") == true)
        {
            Update(MirContext, _registers.MIR);
        }
        else if (e.PropertyName?.StartsWith("MAR") == true)
        {
            Update(MarContext, _registers.MAR);
        }
    }

    private BaseDiagramObject? _lastUpdatedObject;
    
    private void UpdateRegister(REGISTERS reg, ushort value)
    {
        switch (reg)
        {
            case REGISTERS.FLAGS:
                Update(FLAGContext, value);
                break;
            // case REGISTERS.RG:
            //     PCContext.Value = value;
            //     PCContext.IsHighlighted = true;
            //     LastUpdatedObject = PCContext;
            //     break;
            case REGISTERS.SP:
                Update(SPContext, value);
                break;
            case REGISTERS.T:
                Update(TContext, value);
                break;
            case REGISTERS.PC:
                Update(PCContext, value);
                break;
            case REGISTERS.IVR:
                Update(IVRContext, value);
                break;
            case REGISTERS.ADR:
                Update(ADRContext, value);
                break;
            case REGISTERS.MDR:
                Update(MDRContext, value);
                break;
            case REGISTERS.IR:
                Update(IRContext, value);
                break;
            // Handle others as needed
        }
    }

    private void UpdateGpr(GPR gpr, ushort value)
    {
        switch (gpr)
        {
            case GPR.R0:
                Update(R0Context, value);
                break;
            case GPR.R1:
                Update(R1Context, value);
                break;
            case GPR.R2:
                Update(R2Context, value);
                break;
            case GPR.R3:
                Update(R3Context, value);
                break;
            case GPR.R4:
                Update(R4Context, value);
                break;
            case GPR.R5:
                Update(R5Context, value);
                break;
            case GPR.R6:
                Update(R6Context, value);
                break;
            case GPR.R7:
                Update(R7Context, value);
                break;
            case GPR.R8:
                Update(R8Context, value);
                break;
            case GPR.R9:
                Update(R9Context, value);
                break;
            case GPR.R10:
                Update(R10Context, value);
                break;
            case GPR.R11:
                Update(R11Context, value);
                break;
            case GPR.R12:
                Update(R12Context, value);
                break;
            case GPR.R13:
                Update(R13Context, value);
                break;
            case GPR.R14:
                Update(R14Context, value);
                break;
            case GPR.R15:
                Update(R15Context, value);
                break;
        }
    }

    private void Update(RegisterViewModel register, object value)
    {
        register.Value = value;
        _lastUpdatedObject = register;
    }


}
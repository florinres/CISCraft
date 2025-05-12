using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Components.Diagram;

public class DiagramViewModel : ToolViewModel, IDiagramViewModel
{
    public MicroprogramMemoryViewModel MemoryContext { get; } = new();
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
}
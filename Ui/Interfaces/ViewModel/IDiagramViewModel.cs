using Ui.ViewModels.Components.Diagram;

namespace Ui.Interfaces.ViewModel;

public interface IDiagramViewModel : IToolViewModel
{
    RegisterViewModel PCContext { get; }
    RegisterViewModel IVRContext { get; }
    RegisterViewModel TContext { get; }
    RegisterViewModel SPContext { get; }
    RegisterViewModel FLAGContext { get; }
    RegisterViewModel MDRContext { get; }
    RegisterViewModel ADRContext { get; }
    RegisterViewModel IRContext { get; }
    RegisterViewModel R0Context { get; }
    RegisterViewModel R1Context { get; }
    RegisterViewModel R2Context { get; }
    RegisterViewModel R3Context { get; }
    RegisterViewModel R4Context { get; }
    RegisterViewModel R5Context { get; }
    RegisterViewModel R6Context { get; }
    RegisterViewModel R7Context { get; }
    RegisterViewModel R8Context { get; }
    RegisterViewModel R9Context { get; }
    RegisterViewModel R10Context { get; }
    RegisterViewModel R11Context { get; }
    RegisterViewModel R12Context { get; }
    RegisterViewModel R13Context { get; }
    RegisterViewModel R14Context { get; }
    RegisterViewModel R15Context { get; }
    BitBlockViewModel Pd1Context { get; }
    BitBlockViewModel PdMinus1Context { get; }
    BitBlockViewModel Pd0sContext { get; }
}
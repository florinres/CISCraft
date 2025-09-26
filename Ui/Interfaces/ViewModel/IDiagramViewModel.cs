using System.Windows.Media;
using Ui.Models;
using Ui.ViewModels.Components.Diagram;
using Ui.ViewModels.Components.MenuBar;
using Ui.Views.UserControls.Diagram;

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
    BitBlockViewModel BVIContext { get; }
    BitBlockViewModel CContext { get; }
    BitBlockViewModel ZContext { get; }
    BitBlockViewModel SContext { get; }
    BitBlockViewModel VContext { get; }
    void ResetHighlight();
    public void HandleHighlightMpmBox(int index);
    void SetNumberFormat(NumberFormat format);
    void SetDiagramControl(DiagramUserControl diagramControl);
    void HighlightConnectionByName(string connectionName, bool highlight = true, Brush highlightBrush = null);
    void HighlightConnectionByTag(string connectionTag, bool highlight = true, Brush highlightBrush = null);
    void HandleHighlightConnection(ushort flags, string connectionTag, bool highlight = true, Brush highlightBrush = null);
    void HighlightComponentConnections(string componentName, bool highlight = true, Brush highlightBrush = null);
    void HighlightFlagBitConnections(bool highlight = true, Brush highlightBrush = null);
    public void SetIOColor(Brush color, string io = "IO0");
    void UpdateBusValues(ushort sbus, ushort dbus, ushort rbus);
}
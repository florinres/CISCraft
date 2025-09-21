using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using CPU.Business.Models;
using Ui.Interfaces.ViewModel;
using Ui.Models;
using Ui.ViewModels.Components.MenuBar;
using Ui.ViewModels.Generics;
using Ui.Views.UserControls.Diagram;
using Ui.Views.UserControls.Diagram.Components;

namespace Ui.ViewModels.Components.Diagram;

public partial class DiagramViewModel : ToolViewModel, IDiagramViewModel
{
    private readonly RegisterWrapper _registers;
    private DiagramUserControl _diagramControl;
    private Canvas _connectionCanvas;
    private Canvas _overlayCanvas;

    public DiagramViewModel(IMicroprogramViewModel microprogramViewModel, RegisterWrapper registers)
    {
        MemoryContext = microprogramViewModel;
        _registers = registers;
        BindToRegisters();
        SetUpContext();
    }
    
    /// <summary>
    /// Sets the reference to the associated DiagramUserControl
    /// </summary>
    /// <param name="diagramControl">The DiagramUserControl instance</param>
    public void SetDiagramControl(DiagramUserControl diagramControl)
    {
        _diagramControl = diagramControl;
        _connectionCanvas = diagramControl.GetConnectionCanvas();
        _overlayCanvas = diagramControl.GetOverlayCanvas();
    }
    
    /// <summary>
    /// Highlights a connection by its name
    /// </summary>
    /// <param name="connectionName">The name of the connection to highlight</param>
    /// <param name="highlight">Whether to highlight (true) or remove highlight (false)</param>
    /// <param name="highlightBrush">The brush to use for highlighting, defaults to cyan</param>
    public void HighlightConnectionByName(string connectionName, bool highlight = true, Brush highlightBrush = null)
    {
        if (_connectionCanvas == null || _overlayCanvas == null) return;
        
        highlightBrush ??= new SolidColorBrush(Color.FromRgb(0, 208, 255)); // cyan
        
        _diagramControl.Dispatcher.InvokeAsync(() => {
            // First find the connection in ConnectionCanvas
            HighlightableConnector targetConnector = null;
            
            foreach (var child in _connectionCanvas.Children)
            {
                if (child is HighlightableConnector connector && connector.Name == connectionName)
                {
                    targetConnector = connector;
                    break;
                }
            }
            
            if (targetConnector == null) return;
            
            // Look for existing overlay for this connection
            Polyline existingOverlay = null;
            string overlayName = $"overlay_{connectionName}";
            
            foreach (var child in _overlayCanvas.Children)
            {
                if (child is Polyline polyline && polyline.Name == overlayName)
                {
                    existingOverlay = polyline;
                    break;
                }
            }
            
            if (highlight)
            {
                // Create overlay if it doesn't exist yet
                if (existingOverlay == null)
                {
                    existingOverlay = new Polyline
                    {
                        Name = overlayName,
                        Points = new PointCollection(targetConnector.Points),
                        Stroke = highlightBrush,
                        StrokeThickness = 3,
                        IsHitTestVisible = false
                    };
                    
                    if (highlightBrush is SolidColorBrush solidBrush)
                    {
                        existingOverlay.Effect = new DropShadowEffect
                        {
                            Color = solidBrush.Color,
                            BlurRadius = 12,
                            ShadowDepth = 0,
                            Opacity = 0.9
                        };
                    }
                    
                    Panel.SetZIndex(existingOverlay, 1000);
                    _overlayCanvas.Children.Add(existingOverlay);
                }
                else
                {
                    // Update existing overlay
                    existingOverlay.Visibility = Visibility.Visible;
                    existingOverlay.Stroke = highlightBrush;
                    
                    if (highlightBrush is SolidColorBrush solidBrush)
                    {
                        existingOverlay.Effect = new DropShadowEffect
                        {
                            Color = solidBrush.Color,
                            BlurRadius = 12,
                            ShadowDepth = 0,
                            Opacity = 0.9
                        };
                    }
                    
                    Panel.SetZIndex(existingOverlay, 1000);
                }
                
                // Mark the original connector as highlighted (just for state tracking)
                targetConnector.IsHighlighted = true;
            }
            else if (existingOverlay != null)
            {
                // Remove highlighting
                _overlayCanvas.Children.Remove(existingOverlay);
                targetConnector.IsHighlighted = false;
            }
        });
    }
    
    /// <summary>
    /// Highlights all connections related to a specific register or component
    /// </summary>
    /// <param name="componentName">Name of the register or component</param>
    /// <param name="highlight">Whether to highlight (true) or remove highlight (false)</param>
    /// <param name="highlightBrush">Optional custom brush for highlighting</param>
    public void HighlightComponentConnections(string componentName, bool highlight = true, Brush highlightBrush = null)
    {
        if (_connectionCanvas == null || _overlayCanvas == null) return;
        
        highlightBrush ??= new SolidColorBrush(Color.FromRgb(0, 208, 255)); // cyan
        
        _diagramControl.Dispatcher.InvokeAsync(() => {
            // First, if removing highlights, clean up any existing overlays for this component
            if (!highlight)
            {
                List<UIElement> toRemove = new List<UIElement>();
                string overlayPrefix = $"overlay_{componentName}";
                
                foreach (var child in _overlayCanvas.Children)
                {
                    if (child is Polyline polyline && 
                        polyline.Name != null && 
                        polyline.Name.Contains(componentName, StringComparison.OrdinalIgnoreCase))
                    {
                        toRemove.Add(polyline);
                    }
                }
                
                foreach (var element in toRemove)
                {
                    _overlayCanvas.Children.Remove(element);
                }
                
                // Also reset IsHighlighted on original connectors
                foreach (var child in _connectionCanvas.Children)
                {
                    if (child is HighlightableConnector connector && 
                        connector.Name != null && 
                        connector.Name.Contains(componentName, StringComparison.OrdinalIgnoreCase))
                    {
                        connector.IsHighlighted = false;
                    }
                }
                
                return;
            }
            
            // For highlighting, create overlays for each matching connector
            foreach (var child in _connectionCanvas.Children)
            {
                if (child is HighlightableConnector connector && 
                    connector.Name != null && 
                    connector.Name.Contains(componentName, StringComparison.OrdinalIgnoreCase))
                {
                    // Create unique overlay name for this connector
                    string overlayName = $"overlay_{connector.Name}";
                    
                    // Check if overlay already exists
                    bool overlayExists = false;
                    foreach (var overlayChild in _overlayCanvas.Children)
                    {
                        if (overlayChild is Polyline polyline && polyline.Name == overlayName)
                        {
                            overlayExists = true;
                            break;
                        }
                    }
                    
                    if (!overlayExists)
                    {
                        // Create new overlay
                        Polyline overlay = new Polyline
                        {
                            Name = overlayName,
                            Points = new PointCollection(connector.Points),
                            Stroke = highlightBrush,
                            StrokeThickness = 3,
                            IsHitTestVisible = false
                        };
                        
                        if (highlightBrush is SolidColorBrush solidBrush)
                        {
                            overlay.Effect = new DropShadowEffect
                            {
                                Color = solidBrush.Color,
                                BlurRadius = 12,
                                ShadowDepth = 0,
                                Opacity = 0.9
                            };
                        }
                        
                        Panel.SetZIndex(overlay, 1000);
                        _overlayCanvas.Children.Add(overlay);
                        
                        // Mark original connector as highlighted
                        connector.IsHighlighted = true;
                    }
                }
            }
        });
    }
    
    /// <summary>
    /// Highlights all connections between FLAG register and individual flag bits
    /// </summary>
    /// <param name="highlight">Whether to highlight (true) or remove highlight (false)</param>
    /// <param name="highlightBrush">Optional custom brush for highlighting</param>
    public void HighlightFlagBitConnections(bool highlight = true, Brush highlightBrush = null)
    {
        var flagBitNames = new[] { "BVI", "C", "Z", "S", "V" };
        
        foreach (var bitName in flagBitNames)
        {
            string connectionName = $"{bitName}_Flags";
            HighlightConnectionByName(connectionName, highlight, highlightBrush);
        }
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

    public void SetNumberFormat(NumberFormat format)
    {
        DataInContext.Format = format;
        DataOutContext.Format = format;
        PCContext.Format = format;
        IVRContext.Format = format;
        TContext.Format = format;
        SPContext.Format = format;
        FLAGContext.Format = format;
        MDRContext.Format = format;
        ADRContext.Format = format;
        IRContext.Format = format;
        R0Context.Format = format;
        R1Context.Format = format;
        R2Context.Format = format;
        R3Context.Format = format;
        R4Context.Format = format;
        R5Context.Format = format;
        R6Context.Format = format;
        R7Context.Format = format;
        R8Context.Format = format;
        R9Context.Format = format;
        R10Context.Format = format;
        R11Context.Format = format;
        R12Context.Format = format;
        R13Context.Format = format;
        R14Context.Format = format;
        R15Context.Format = format;
        MirContext.Format = format;
        MarContext.Format = format;
    }

    public void ResetHighlight()
    {
        // Reset context highlights
        foreach (var context in Contexts)
        {
            context.IsHighlighted = false;
        }
        
        // Clear all overlays from OverlayCanvas
        if (_overlayCanvas != null)
        {
            _diagramControl.Dispatcher.InvokeAsync(() => {
                // Find and remove all overlay elements (those with "overlay_" prefix in name)
                List<UIElement> toRemove = new List<UIElement>();
                
                foreach (var child in _overlayCanvas.Children)
                {
                    if (child is Polyline polyline && 
                        polyline.Name != null && 
                        polyline.Name.StartsWith("overlay_"))
                    {
                        toRemove.Add(polyline);
                    }
                }
                
                foreach (var element in toRemove)
                {
                    _overlayCanvas.Children.Remove(element);
                }
                
                // Reset IsHighlighted flag on all connectors
                if (_connectionCanvas != null)
                {
                    foreach (var child in _connectionCanvas.Children)
                    {
                        if (child is HighlightableConnector connector)
                        {
                            connector.IsHighlighted = false;
                        }
                    }
                }
            });
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
                // Update individual flag bits
                Update(BVIContext, (value & 0x0080) != 0 ? 1 : 0); // BVI is bit 7
                Update(CContext, (value & 0x0008) != 0 ? 1 : 0);   // C is bit 3
                Update(ZContext, (value & 0x0004) != 0 ? 1 : 0);   // Z is bit 2
                Update(SContext, (value & 0x0002) != 0 ? 1 : 0);   // S is bit 1
                Update(VContext, (value & 0x0001) != 0 ? 1 : 0);   // V is bit 0
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
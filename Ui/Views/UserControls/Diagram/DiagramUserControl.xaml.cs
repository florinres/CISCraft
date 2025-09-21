using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Ui.Interfaces.ViewModel;
using Ui.Models;
using Ui.ViewModels.Components.Diagram;
using Ui.Views.UserControls.Diagram.Components;

namespace Ui.Views.UserControls.Diagram;

public partial class DiagramUserControl : UserControl
{
    private Point _lastPanPoint;
    private bool _isPanning = false;
    private readonly TranslateTransform _panTransform = new();
    private readonly ScaleTransform _zoomTransform = new();
    private readonly TransformGroup _transformGroup = new();
    private Canvas _overlayCanvas;
    
    /// <summary>
    /// Gets the ConnectionCanvas used for drawing connections between components
    /// </summary>
    private Canvas _connectionCanvas;

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(IDiagramViewModel), typeof(DiagramUserControl),
            new PropertyMetadata(null, OnViewModelChanged));

    public IDiagramViewModel ViewModel
    {
        get => (IDiagramViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DiagramUserControl control && e.NewValue is IDiagramViewModel vm)
        {
            control.DataContext = vm;
            vm.SetDiagramControl(control);
        }
    }

    public DiagramUserControl()
    {
        InitializeComponent();
        
        // Initialize canvas reference
        _connectionCanvas = this.FindName("ConnectionCanvas") as Canvas;
        _overlayCanvas = this.FindName("OverlayCanvas") as Canvas;

        MainDiagramGrid.Loaded += (s, e) =>
        {
            Dispatcher.BeginInvoke(new Action(DrawConnections), System.Windows.Threading.DispatcherPriority.Loaded);
        };

        _transformGroup.Children.Add(_panTransform);
        _transformGroup.Children.Add(_zoomTransform);
        MainDiagramGrid.RenderTransform = _transformGroup;

        MainDiagramGrid.MouseWheel += Window_MouseWheel;
        MainDiagramGrid.MouseLeftButtonDown += Pan_MouseLeftButtonDown;
        MainDiagramGrid.MouseLeftButtonUp += Pan_MouseLeftButtonUp;
        MainDiagramGrid.MouseMove += Pan_MouseMove;

    }
    
    /// <summary>
    /// Highlights all connections related to a specific register or component
    /// </summary>
    /// <param name="componentName">Name of the register or component</param>
    /// <param name="highlight">Whether to highlight (true) or remove highlight (false)</param>
    /// <param name="highlightBrush">Optional custom brush for highlighting</param>
    public void HighlightComponentConnections(string componentName, bool highlight = true, Brush highlightBrush = null)
    {
        // This method is now handled by the ViewModel
        if (ViewModel != null)
        {
            ViewModel.HighlightComponentConnections(componentName, highlight, highlightBrush);
        }
    }
    
    /// <summary>
    /// Highlights a connection by its name
    /// </summary>
    /// <param name="connectionName">Name of the connection to highlight</param>
    /// <param name="highlight">Whether to highlight (true) or remove highlight (false)</param>
    /// <param name="highlightBrush">Optional custom brush for highlighting</param>
    public void HighlightConnectionByName(string connectionName, bool highlight = true, Brush highlightBrush = null)
    {
        // This method is now handled by the ViewModel
        if (ViewModel != null)
        {
            ViewModel.HighlightConnectionByName(connectionName, highlight, highlightBrush);
        }
    }
    
    /// <summary>
    /// Highlights all connections between FLAG register and individual flag bits
    /// </summary>
    /// <param name="highlight">Whether to highlight (true) or remove highlight (false)</param>
    /// <param name="highlightBrush">Optional custom brush for highlighting</param>
    public void HighlightFlagBitConnections(bool highlight = true, Brush highlightBrush = null)
    {
        // This method is now handled by the ViewModel
        if (ViewModel != null)
        {
            ViewModel.HighlightFlagBitConnections(highlight, highlightBrush);
        }
    }

    private void Pan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isPanning = true;
        _lastPanPoint = e.GetPosition(this);
        MainDiagramGrid.CaptureMouse();
        MainDiagramGrid.Cursor = Cursors.Hand;
    }

    private void Pan_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isPanning = false;
        MainDiagramGrid.ReleaseMouseCapture();
        MainDiagramGrid.Cursor = Cursors.Arrow;
    }
    
    /// <summary>
    /// Gets the ConnectionCanvas for drawing connections between components
    /// </summary>
    /// <returns>The ConnectionCanvas instance</returns>
    public Canvas GetConnectionCanvas()
    {
        return _connectionCanvas;
    }
    
    /// <summary>
    /// Gets the OverlayCanvas for highlighting connections between components
    /// </summary>
    /// <returns>The OverlayCanvas instance</returns>
    public Canvas GetOverlayCanvas()
    {
        return _overlayCanvas;
    }

    private void Pan_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning) return;
        var currentPoint = e.GetPosition(this);
        var delta = currentPoint - _lastPanPoint;
        _lastPanPoint = currentPoint;

        _panTransform.X += delta.X;
        _panTransform.Y += delta.Y;
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;

        const double zoomStep = 0.1;
        const double minZoom = .8;
        const double maxZoom = 2.0;

        double oldZoom = _zoomTransform.ScaleX;
        double newZoom = e.Delta > 0 ? oldZoom + zoomStep : oldZoom - zoomStep;
        newZoom = Math.Max(minZoom, Math.Min(maxZoom, newZoom));

        // Zoom centered on mouse
        var mousePos = e.GetPosition(MainDiagramGrid);

        // Adjust pan so zoom is centered on mouse
        var relativeX = mousePos.X * (newZoom / oldZoom) - mousePos.X;
        var relativeY = mousePos.Y * (newZoom / oldZoom) - mousePos.Y;
        _panTransform.X -= relativeX;
        _panTransform.Y -= relativeY;

        _zoomTransform.ScaleX = newZoom;
        _zoomTransform.ScaleY = newZoom;

        e.Handled = true;
    }

    #region Bullshit

    private void DrawConnections()
    {
        _connectionCanvas.Children.Clear(); // optional, if redrawing

        var sBusEdges = SBus.GetEdges(MainDiagramGrid);
        var dBusEdges = DBus.GetEdges(MainDiagramGrid);
        var rBusLeftInnerEdge = RBus.GetEdgeAbsolute(PathSide.Left, EdgeType.Inner, MainDiagramGrid);

        List<RegisterBlock> registersCollection =
        [
            Ivr,
            Pc,
            T,
            Sp,
            Flags,
            Mdr,
            Adr
        ];

        foreach (RegisterBlock registerBlock in registersCollection)
        {
            var connectionPoints = RegisterBlockAddConnections(registerBlock, sBusEdges, dBusEdges);
            var rBusHighlight = new HighlightableConnector
            {
                Points =
                [
                    connectionPoints.MidLeft with { X = connectionPoints.MidLeft.X - 2 },
                    connectionPoints.MidLeft with { X = rBusLeftInnerEdge - 1 }
                ],
                StrokeThickness = 2,
                Name = $"Pm{registerBlock.Name}"
            };
            Panel.SetZIndex(rBusHighlight, -1);
            AddWireEffects(rBusHighlight, rBusHighlight.Name);
            _connectionCanvas.Children.Add(rBusHighlight);
        }

        // Add connections for IR and other components
        RegisterBlockAddConnections(Ir, sBusEdges, dBusEdges);
        AddAddressConnections();
        AddDataOutConnections();
        AddDataInConnections();
        AddDataOutIrConnections();
        AddGeneralRegistersConnections();
        ConnectIOsToSie();
        AddPdsConnections();
        AddAluToRBusConnection();
    }

    public void ConnectIOsToSie()
    {
        var ios = new[] { IO0, IO1, IO2, IO3 };

        var canvas = _connectionCanvas;

        foreach (var io in ios)
        {
            // Get the bounding rectangle of IO relative to the canvas
            Rect ioBounds = io.TransformToVisual(canvas)
                .TransformBounds(new Rect(0, 0, io.ActualWidth, io.ActualHeight));
            Rect sieBounds = Sie.TransformToVisual(canvas)
                .TransformBounds(new Rect(0, 0, Sie.ActualWidth, Sie.ActualHeight));

            HighlightableConnector connector = new HighlightableConnector
            {
                Stroke = Brushes.White,
                StrokeThickness = 2,
                IsHighlighted = false
            };

            // Determine start and end points from edges
            Point ioEdge, sieEdge;

            double horizontalInset = 30; // how much the line moves horizontally before going vertical

            if (io == IO0) // leftmost → S-shape
                ioEdge = new Point(ioBounds.Right, ioBounds.Top + ioBounds.Height / 2);
            else if (io == IO1) // middle-left → elbow
                ioEdge = new Point(ioBounds.Right, ioBounds.Top + ioBounds.Height / 2);
            else if (io == IO2) // middle-right → elbow
                ioEdge = new Point(ioBounds.Left, ioBounds.Top + ioBounds.Height / 2);
            else // IO3 → rightmost → S-shape
                ioEdge = new Point(ioBounds.Left, ioBounds.Top + ioBounds.Height / 2);

            // SIE: top edge center
            sieEdge = new Point(sieBounds.Left + sieBounds.Width / 2, sieBounds.Top);

            if (io == IO1 || io == IO2)
            {
                // Add small horizontal segment toward SIE before going vertical
                double intermediateX = ioEdge.X + (io == IO1 ? horizontalInset : -horizontalInset);

                connector.Points = new PointCollection
                {
                    ioEdge, // start at inner edge
                    new Point(intermediateX, ioEdge.Y), // horizontal closer to SIE
                    new Point(intermediateX, sieEdge.Y), // vertical down
                    sieEdge // connect to SIE
                };
            }
            else // IO0 or IO3 → S-shaped
            {
                double offset = 40;
                connector.Points = new PointCollection
                {
                    ioEdge,
                    new Point(ioEdge.X + (io == IO0 ? offset : -offset), ioEdge.Y),
                    new Point(ioEdge.X + (io == IO0 ? offset : -offset), ioEdge.Y + offset / 2),
                    new Point(sieEdge.X + (io == IO0 ? -offset : offset), ioEdge.Y + offset / 2),
                    new Point(sieEdge.X + (io == IO0 ? -offset : offset), sieEdge.Y)
                };
            }

            canvas.Children.Add(connector);
            Point sieBottom = new Point(sieBounds.Left + sieBounds.Width / 2, sieBounds.Bottom);

            // Get IVR top center
            Rect ivrBounds = Ivr.TransformToVisual(canvas)
                .TransformBounds(new Rect(0, 0, Ivr.ActualWidth, Ivr.ActualHeight));
            Point ivrTop = new Point(ivrBounds.Left + ivrBounds.Width / 2, ivrBounds.Top);

            HighlightableConnector connector2 = new HighlightableConnector
            {
                Stroke = Brushes.White,
                StrokeThickness = 2,
                IsHighlighted = false
            };

            connector2.Points = new PointCollection
            {
                sieBottom,
                ivrTop
            };

            canvas.Children.Add(connector2);
        }
    }
    
    private void AddGeneralRegistersConnections()
    {
        //R7 just because it is in the center, no particular reason
        var connectionPoints = R7.GetConnectionPoints(MainDiagramGrid);

        var sBusEdges = SBus.GetEdges(MainDiagramGrid);
        var dBusEdges = DBus.GetEdges(MainDiagramGrid);
        var rBusRightInnerEdge = RBus.GetEdgeAbsolute(PathSide.Right, EdgeType.Inner, MainDiagramGrid);
        const int offset = 120;

        var sBusHighlight = new HighlightableConnector
        {
            Points =
            [
                connectionPoints.LeftMinusOffset with { X = connectionPoints.LeftMinusOffset.X - 2, Y = connectionPoints.LeftMinusOffset.Y - offset },
                connectionPoints.LeftMinusOffset with { X = sBusEdges.Right, Y = connectionPoints.LeftMinusOffset.Y - offset },
            ],
            StrokeThickness = 2,
            Name = $"PdRGs"
        };
        var dBusHighlight = new HighlightableConnector
        {
            Points =
            [
                connectionPoints.LeftPlusOffset with { X = connectionPoints.LeftPlusOffset.X - 2, Y = connectionPoints.LeftPlusOffset.Y - offset },
                connectionPoints.LeftPlusOffset with { X = dBusEdges.Right, Y = connectionPoints.LeftPlusOffset.Y - offset },
            ],
            StrokeThickness = 2,
            Name = $"PdRGd"
        };

        var foo = new PointCollection
        {
            connectionPoints.MidRight with { X = connectionPoints.MidRight.X + 2 },
            connectionPoints.MidRight with { X = rBusRightInnerEdge }
        };

        var rBusHighLight = new HighlightableConnector
        {
            Points = foo,
            StrokeThickness = 2,
            Name = $"PmRG"
        };

        AddWireEffects(sBusHighlight, sBusHighlight.Name);
        AddWireEffects(dBusHighlight, dBusHighlight.Name);
        AddWireEffects(rBusHighLight, rBusHighLight.Name);
        _connectionCanvas.Children.Add(sBusHighlight);
        _connectionCanvas.Children.Add(dBusHighlight);
        _connectionCanvas.Children.Add(rBusHighLight);
    }

    private void AddDataOutIrConnections()
    {
        var connectionPointsDataOut = DataOut.GetConnectionPoints(MainDiagramGrid);
        var connectionPointsIr = Ir.GetConnectionPoints(MainDiagramGrid);

        var offset = 40;

        var connection = new HighlightableConnector
        {
            Points =
            [
                connectionPointsDataOut.MidLeft with { X = connectionPointsDataOut.MidLeft.X - 2 },
                connectionPointsDataOut.MidLeft with { X = connectionPointsDataOut.MidLeft.X - offset },
                connectionPointsIr.MidLeft with { X = connectionPointsDataOut.MidLeft.X - offset },
                connectionPointsIr.MidLeft with { X = connectionPointsIr.MidLeft.X - 2 }
            ],
            StrokeThickness = 2,
            Name = $"{DataOut.Name}_{Ir.Name}"
        };

        AddWireEffects(connection, $"{connection.Name}");
        _connectionCanvas.Children.Add(connection);
    }

    private void AddAddressConnections()
    {
        var connectionPointsAdress = Address.GetConnectionPoints(MainDiagramGrid);
        var connectionPointsAdr = Adr.GetConnectionPoints(MainDiagramGrid);

        var offset = 10;

        var connection = new HighlightableConnector
        {
            Points =
            [
                connectionPointsAdress.MidRight with { X = connectionPointsAdress.MidRight.X - 2},
                connectionPointsAdress.MidRight with { X = connectionPointsAdress.MidRight.X + 30},
                connectionPointsAdress.TopOffsetPlus with { Y = connectionPointsAdress.TopOffsetPlus.Y - offset - 2, X = connectionPointsAdress.RightMinusOffset.X + 30},
                new(connectionPointsAdr.RightPlusOffset.X + offset, connectionPointsAdress.TopOffsetPlus.Y - offset - 2),
                connectionPointsAdr.RightPlusOffset with { X = connectionPointsAdr.RightPlusOffset.X + offset }
            ],
            StrokeThickness = 2,
            Name = $"{Adr.Name}"
        };

        AddWireEffects(connection, $"{connection.Name}");
        _connectionCanvas.Children.Add(connection);
    }

    private void AddDataOutConnections()
    {
        var connectionPointsDataOut = DataOut.GetConnectionPoints(MainDiagramGrid);
        var connectionPointsMdr = Mdr.GetConnectionPoints(MainDiagramGrid);

        var offset = 150;

        var connection = new HighlightableConnector
        {
            Points =
            [
                connectionPointsDataOut.MidRight with { X = connectionPointsDataOut.MidRight.X - 2},
                connectionPointsDataOut.MidRight with
                {
                    X = connectionPointsDataOut.MidRight.X + offset
                },
                connectionPointsMdr.MidLeft with { X = connectionPointsDataOut.MidRight.X + offset }
            ],
            StrokeThickness = 2,
            Name = $"DataOut"
        };

        AddWireEffects(connection, $"{connection.Name}");
        _connectionCanvas.Children.Add(connection);
    }

    private void AddDataInConnections()
    {
        var connectionPointsDataIn = DataIn.GetConnectionPoints(MainDiagramGrid);
        var connectionPointsMdr = Mdr.GetConnectionPoints(MainDiagramGrid);

        var offset = 8;

        var connection = new HighlightableConnector
        {
            Points =
            [
                connectionPointsDataIn.MidRight with { X = connectionPointsDataIn.MidRight.X - 2},
                connectionPointsDataIn.MidRight with { X = connectionPointsDataIn.MidRight.X + 30},
                connectionPointsDataIn.TopOffsetPlus with { Y = connectionPointsMdr.TopOffsetPlus.Y - offset, X = connectionPointsDataIn.RightMinusOffset.X + 30},
                new Point(connectionPointsMdr.RightMinusOffset.X + offset, connectionPointsMdr.TopOffsetPlus.Y - offset),
                connectionPointsMdr.RightMinusOffset with { X = connectionPointsMdr.RightMinusOffset.X + offset }
            ],
            StrokeThickness = 2,
            Name = $"DataIn"
        };

        AddWireEffects(connection, $"{connection.Name}");
        _connectionCanvas.Children.Add(connection);
    }

    private void AddPdsConnections()
    {
        var flagConnections = Flags.GetConnectionPoints(MainDiagramGrid);

        var relevantY = flagConnections.MidBottom.Y;
        
        var pdCollection = new List<BitBlock>()
        {
            BVI,
            C,
            Z,
            S,
            V
        };

        foreach (var bitBlock in pdCollection)
        {
            var bitBlockConnectionPoint = bitBlock.GetConnectionPointRelativeTo(MainDiagramGrid);

            var connection = new HighlightableConnector
            {
                Points =
                [
                    bitBlockConnectionPoint with { X = bitBlockConnectionPoint.X + 3},
                    bitBlockConnectionPoint with { X = bitBlockConnectionPoint.X + 3, Y = relevantY - 2}
                ],
                StrokeThickness = 2,
                Name = $"{bitBlock.Name}_Flags"
            };

            AddWireEffects(connection, $"{connection.Name}");
            _connectionCanvas.Children.Add(connection);
        }
    }

    private RegisterBlockConnectionPoints RegisterBlockAddConnections(RegisterBlock registerBlock,
        (double Left, double Right) sBusEdges, (double Left, double Right) dBusEdges)
    {
        var connectionPoints = registerBlock.GetConnectionPoints(MainDiagramGrid);

        var sBusHighlight = new HighlightableConnector
        {
            Points =
            [
                connectionPoints.RightMinusOffset with { X = connectionPoints.RightMinusOffset.X - 2 },
                connectionPoints.RightMinusOffset with { X = sBusEdges.Left + 1 },
            ],
            StrokeThickness = 2,
            Name = $"Pd{registerBlock.Name}S"
        };
        var dBusHighlight = new HighlightableConnector
        {
            Points =
            [
                connectionPoints.RightPlusOffset with { X = connectionPoints.RightPlusOffset.X - 2, Y = connectionPoints.RightPlusOffset.Y - 2 },
                connectionPoints.RightPlusOffset with { X = dBusEdges.Left + 1, Y = connectionPoints.RightPlusOffset.Y - 2 },
            ],
            StrokeThickness = 2,
            Name = $"Pd{registerBlock.Name}D"
        };
        AddWireEffects(sBusHighlight, sBusHighlight.Name);
        AddWireEffects(dBusHighlight, dBusHighlight.Name);

        _connectionCanvas.Children.Add(sBusHighlight);
        _connectionCanvas.Children.Add(dBusHighlight);

        return connectionPoints;
    }

    private void AddAluToRBusConnection()
    {
        var aluPts = Alu.GetConnectionPoints(MainDiagramGrid);
        var rBusLeftInnerEdge = RBus.GetEdgeAbsolute(PathSide.Bottom, EdgeType.Inner, MainDiagramGrid);

        var conn = new HighlightableConnector
        {
            StrokeThickness = 2,
            Name = "PdAlu",
            Points = new PointCollection
            {
                aluPts.MidBottom with { Y = aluPts.MidBottom.Y - 10},
                aluPts.MidBottom with { Y = aluPts.MidBottom.Y + 10},
            }
        };
        Panel.SetZIndex(conn, -1);
        AddWireEffects(conn, conn.Name);
        _connectionCanvas.Children.Add(conn);
    }
    void AddWireEffects(HighlightableConnector wire, string toolTipText)
    {
        AttachTooltip(wire, toolTipText);
        WireHoverOverlay(wire, Brushes.Red);
    }
    
    private static void AttachTooltip(FrameworkElement el, string text)
    {
        ToolTipService.SetPlacement(el, PlacementMode.MousePoint);
        ToolTipService.SetInitialShowDelay(el, 1);
        ToolTipService.SetBetweenShowDelay(el, 0);
        ToolTipService.SetShowDuration(el, 6000);

        el.ToolTip = new TextBlock { Text = text };
    }

    private void WireHoverOverlay(HighlightableConnector target,
                              Brush hoverBrush = null,
                              double hoverThickness = 3,
                              double glowBlur = 12,
                              double glowOpacity = 0.9)
    {
        hoverBrush ??= new SolidColorBrush(Color.FromRgb(0, 208, 255)); // cyan

        Polyline overlay = null;

        void EnsureOverlay()
        {
            if (overlay != null) return;

            overlay = new Polyline
            {
                // clone the geometry
                Points = new PointCollection(target.Points),
                Stroke = hoverBrush,
                StrokeThickness = hoverThickness,
                IsHitTestVisible = false,   // let the original receive mouse
                SnapsToDevicePixels = true
            };

            // glow
            if (hoverBrush is SolidColorBrush scb)
            {
                overlay.Effect = new DropShadowEffect
                {
                    Color = scb.Color,
                    BlurRadius = glowBlur,
                    ShadowDepth = 0,
                    Opacity = glowOpacity
                };
            }

            // place above the source
            Panel.SetZIndex(overlay, 999);
            OverlayCanvas.Children.Add(overlay);
        }

        // keep overlay in sync if layout changes (e.g., you redraw connections)
        target.LayoutUpdated += (_, __) =>
        {
            if (overlay != null)
            {
                overlay.Points = new PointCollection(target.Points);
                Panel.SetZIndex(overlay, 999);
            }
        };

        target.Unloaded += (_, __) =>
        {
            if (overlay != null)
            {
                OverlayCanvas.Children.Remove(overlay);
                overlay = null;
            }
        };

        target.MouseEnter += (_, __) =>
        {
            EnsureOverlay();
            overlay.Visibility = Visibility.Visible;
            Panel.SetZIndex(target, 999); // make sure tooltip stays on top
            Panel.SetZIndex(overlay, 1000); // make sure tooltip stays on top
            target.Cursor = Cursors.Hand;
        };

        target.MouseLeave += (_, __) =>
        {
            if (overlay != null) overlay.Visibility = Visibility.Collapsed;
            Panel.SetZIndex(target, -1);
            Panel.SetZIndex(overlay, 999); // make sure tooltip stays on top
            target.ClearValue(CursorProperty);
        };
    }

    #endregion
}
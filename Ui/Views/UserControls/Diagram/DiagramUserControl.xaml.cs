using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        }
    }

    public DiagramUserControl()
    {
        InitializeComponent();

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
        ConnectionCanvas.Children.Clear(); // optional, if redrawing

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
                    connectionPoints.MidLeft,
                    connectionPoints.MidLeft with { X = rBusLeftInnerEdge }
                ],
                StrokeThickness = 2,
                Name = $"{registerBlock.Name}_RBus"
            };
            ConnectionCanvas.Children.Add(rBusHighlight);
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
    }

    public void ConnectIOsToSie()
    {
        var ios = new[] { IO0, IO1, IO2, IO3 };

        var canvas = ConnectionCanvas;

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

            double horizontalInset = 40; // how much the line moves horizontally before going vertical

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
                double offset = 60;
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

        var sBusHighlight = new HighlightableConnector
        {
            Points =
            [
                connectionPoints.LeftMinusOffset,
                connectionPoints.LeftMinusOffset with { X = sBusEdges.Right },
            ],
            StrokeThickness = 2,
            Name = $"R_SBus"
        };
        var dBusHighlight = new HighlightableConnector
        {
            Points =
            [
                connectionPoints.LeftPlusOffset,
                connectionPoints.LeftPlusOffset with { X = dBusEdges.Right },
            ],
            StrokeThickness = 2,
            Name = $"R_DBus"
        };

        var foo = new PointCollection
        {
            connectionPoints.MidRight,
            connectionPoints.MidRight with { X = rBusRightInnerEdge }
        };

        var rBusHighLight = new HighlightableConnector
        {
            Points = foo,
            StrokeThickness = 2,
            Name = $"R_RBus"
        };

        ConnectionCanvas.Children.Add(sBusHighlight);
        ConnectionCanvas.Children.Add(dBusHighlight);
        ConnectionCanvas.Children.Add(rBusHighLight);
    }

    private void AddDataOutIrConnections()
    {
        var connectionPointsDataOut = DataOut.GetConnectionPoints(MainDiagramGrid);
        var connectionPointsIr = Ir.GetConnectionPoints(MainDiagramGrid);

        var offset = 7;

        var connection = new HighlightableConnector
        {
            Points =
            [
                connectionPointsDataOut.MidLeft,
                connectionPointsDataOut.MidLeft with { X = connectionPointsDataOut.MidLeft.X - offset },
                connectionPointsIr.MidLeft with { X = connectionPointsDataOut.MidLeft.X - offset },
                connectionPointsIr.MidLeft
            ],
            StrokeThickness = 2,
            Name = $"{DataOut.Name}_{Ir.Name}"
        };

        ConnectionCanvas.Children.Add(connection);
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
                connectionPointsAdress.TopOffsetPlus,
                connectionPointsAdress.TopOffsetPlus with { Y = connectionPointsAdress.TopOffsetPlus.Y - offset },
                new(connectionPointsAdr.RightPlusOffset.X + offset, connectionPointsAdress.TopOffsetPlus.Y - offset),
                connectionPointsAdr.RightPlusOffset with { X = connectionPointsAdr.RightPlusOffset.X + offset }
            ],
            StrokeThickness = 2,
            Name = $"{Adr.Name}_DBus"
        };

        ConnectionCanvas.Children.Add(connection);
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
                connectionPointsDataOut.MidRight,
                connectionPointsDataOut.MidRight with
                {
                    X = connectionPointsDataOut.MidRight.X + offset
                },
                connectionPointsMdr.MidLeft with { X = connectionPointsDataOut.MidRight.X + offset }
            ],
            StrokeThickness = 2,
            Name = $"{Mdr.Name}_RBus"
        };

        ConnectionCanvas.Children.Add(connection);
    }

    private void AddDataInConnections()
    {
        var connectionPointsDataIn = DataIn.GetConnectionPoints(MainDiagramGrid);
        var connectionPointsMdr = Mdr.GetConnectionPoints(MainDiagramGrid);

        var offset = 10;

        var connection = new HighlightableConnector
        {
            Points =
            [
                connectionPointsDataIn.TopOffsetPlus,
                connectionPointsDataIn.TopOffsetPlus with { Y = connectionPointsMdr.TopOffsetPlus.Y - offset },
                new Point(connectionPointsMdr.RightMinusOffset.X + offset,
                    connectionPointsMdr.TopOffsetPlus.Y - offset),
                connectionPointsMdr.RightMinusOffset with { X = connectionPointsMdr.RightMinusOffset.X + offset }
            ],
            StrokeThickness = 2,
            Name = $"{Mdr.Name}_SBus"
        };

        ConnectionCanvas.Children.Add(connection);
    }

    private void AddPdsConnections()
    {
        var flagConnections = Flags.GetConnectionPoints(MainDiagramGrid);

        var relevantY = flagConnections.MidBottom.Y;
        
        var pdCollection = new List<BitBlock>()
        {
            Pd0s,
            Pd1,
            Pdx,
            Pdy,
            PdMinus1
        };

        foreach (var bitBlock in pdCollection)
        {
            var bitBlockConnectionPoint = bitBlock.GetConnectionPointRelativeTo(MainDiagramGrid);

            var connection = new HighlightableConnector
            {
                Points =
                [
                    bitBlockConnectionPoint,
                    bitBlockConnectionPoint with { Y = relevantY }
                ],
                StrokeThickness = 2,
                Name = $"{bitBlock.Name}_{Flags.Name}"
            };
            
            ConnectionCanvas.Children.Add(connection);
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
                connectionPoints.RightMinusOffset,
                connectionPoints.RightMinusOffset with { X = sBusEdges.Left },
            ],
            StrokeThickness = 2,
            Name = $"{registerBlock.Name}_SBus"
        };
        var dBusHighlight = new HighlightableConnector
        {
            Points =
            [
                connectionPoints.RightPlusOffset,
                connectionPoints.RightPlusOffset with { X = dBusEdges.Left },
            ],
            StrokeThickness = 2,
            Name = $"{registerBlock.Name}_DBus"
        };

        ConnectionCanvas.Children.Add(sBusHighlight);
        ConnectionCanvas.Children.Add(dBusHighlight);

        return connectionPoints;
    }

    #endregion
}
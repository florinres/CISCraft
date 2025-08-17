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
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;

        const double zoomStep = 0.1;
        var newZoom = ViewModel.ZoomFactor;

        if (e.Delta > 0)
        {
            newZoom += zoomStep;
        }
        else
        {
            newZoom = Math.Max(0.1, newZoom - zoomStep);
        }

        ViewModel.ZoomFactor = newZoom;
        e.Handled = true;
    }

    private Point _scrollStartPoint;
    private Point _scrollStartOffset;
    private bool _isDragging;

    private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _scrollStartPoint = e.GetPosition(DiagramScrollViewer);
        _scrollStartOffset.X = DiagramScrollViewer.HorizontalOffset;
        _scrollStartOffset.Y = DiagramScrollViewer.VerticalOffset;
        _isDragging = true;
        DiagramScrollViewer.CaptureMouse();
        DiagramScrollViewer.Cursor = Cursors.Hand;
    }

    private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;

        Point currentPoint = e.GetPosition(DiagramScrollViewer);
        Vector delta = currentPoint - _scrollStartPoint;

        DiagramScrollViewer.ScrollToHorizontalOffset(_scrollStartOffset.X - delta.X);
        DiagramScrollViewer.ScrollToVerticalOffset(_scrollStartOffset.Y - delta.Y);
    }

    private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;

        _isDragging = false;
        DiagramScrollViewer.ReleaseMouseCapture();
        DiagramScrollViewer.Cursor = Cursors.Arrow;
    }

    public void CenterOn(Point position)
    {
        DiagramScrollViewer.ScrollToHorizontalOffset(position.X - DiagramScrollViewer.ViewportWidth / 2);
        DiagramScrollViewer.ScrollToVerticalOffset(position.Y - DiagramScrollViewer.ViewportHeight / 2);
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

        //bullshit
        RegisterBlockAddConnections(Ir, sBusEdges, dBusEdges);
        AddAddressConnections();
        AddDataOutConnections();
        AddDataInConnections();
        AddDataOutIrConnections();
        AddGeneralRegostersConnections();
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
                    // sieEdge
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
    
    private void AddGeneralRegostersConnections()
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
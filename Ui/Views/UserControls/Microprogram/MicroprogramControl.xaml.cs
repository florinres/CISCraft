using System.Windows.Controls;
using System.Windows.Input;
using Ui.ViewModels.Components.Diagram;
using Ui.ViewModels.Components.Microprogram;

namespace Ui.Views.UserControls.Microprogram;

public partial class MicroprogramControl : UserControl
{
    
    public new static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(
            "FontSize", typeof(double), typeof(MicroprogramControl),
            new PropertyMetadata(12.0));

    public new double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public MicroprogramControl()
    {
        InitializeComponent();

        var dataCOntext = new MicroprogramViewModel();
        
        for (int i = 0; i < 16; i++)
        {
            dataCOntext.Rows.Add(new MicroprogramMemoryViewModel
            {
                Address = i,
                Item0 = i switch
                {
                    0 => "NOP",
                    1 => "LOAD A",
                    2 => "LOAD B",
                    3 => "ADD",
                    4 => "SUB",
                    5 => "AND",
                    6 => "OR",
                    7 => "XOR",
                    8 => "JUMP",
                    9 => "CALL",
                    10 => "RET",
                    11 => "PUSH",
                    12 => "POP",
                    13 => "CMP",
                    14 => "JZ",
                    15 => "HALT",
                    _ => "NOP"
                },
                Item1 = $"IR{i:X}", // e.g. IR0, IR1, ..., IRF
                Item2 = i % 3 == 0 ? "FLAGS" : "ALU",
                Item3 = i % 2 == 0 ? "EN" : "DIS",
                Item4 = i % 4 == 0 ? "PM_SET" : "PM_CLR",
                Item5 = (i % 2 == 0) ? "(CIN+COND)" : "(Z+N)",
                Item6 = i % 3 == 0 ? "WRITE" : "READ",
                Item7 = i switch
                {
                    0 => "IF Z JUMP 10",
                    1 => "MOV A, B",
                    2 => "INC A",
                    3 => "DEC B",
                    4 => "SHL A",
                    5 => "SHR B",
                    6 => "IF N JUMP 0A",
                    7 => "CLR FLAGS",
                    8 => "MOV M[ADDR], A",
                    9 => "MOV A, M[ADDR]",
                    10 => "CALL 04",
                    11 => "RET",
                    12 => "JMP 0F",
                    13 => "PUSH A",
                    14 => "POP A",
                    15 => "END",
                    _ => ""
                }
            });
        }
        DataContext = dataCOntext;
    }

    private Point _scrollStartPoint;
    private Point _scrollStartOffset;
    private bool _isDragging;
    private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _scrollStartPoint = e.GetPosition(MicroprogramScrollViewer);
        _scrollStartOffset.X = MicroprogramScrollViewer.HorizontalOffset;
        _scrollStartOffset.Y = MicroprogramScrollViewer.VerticalOffset;
        _isDragging = true;
        MicroprogramScrollViewer.CaptureMouse();
        MicroprogramScrollViewer.Cursor = Cursors.Hand;
    }

    private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        
        Point currentPoint = e.GetPosition(MicroprogramScrollViewer);
        Vector delta = currentPoint - _scrollStartPoint;

        MicroprogramScrollViewer.ScrollToHorizontalOffset(_scrollStartOffset.X - delta.X);
        MicroprogramScrollViewer.ScrollToVerticalOffset(_scrollStartOffset.Y - delta.Y);
    }

    private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        
        _isDragging = false;
        MicroprogramScrollViewer.ReleaseMouseCapture();
        MicroprogramScrollViewer.Cursor = Cursors.Arrow;
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;
        
        FontSize += e.Delta > 0 ? 1 : -1;
        if (FontSize < 8) FontSize = 8;       // Min font size
        if (FontSize > 40) FontSize = 40;     // Max font size
        e.Handled = true;
    }
}
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media;

public class HighlightCurrentLineBackgroundRenderer : IBackgroundRenderer
{
    private readonly TextEditor _editor;
    private int _lineNumber;
    private readonly SolidColorBrush _highlightBrush;

    public HighlightCurrentLineBackgroundRenderer(TextEditor editor, int lineNumber, Color color)
    {
        _editor = editor;
        _lineNumber = lineNumber;
        _highlightBrush = new SolidColorBrush(color);
        _highlightBrush.Freeze();
    }

    public void SetLine(int lineNumber)
    {
        _lineNumber = lineNumber;
        _editor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
    }

    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (_lineNumber <= 0) return;
        textView.EnsureVisualLines();

        var line = _editor.Document.GetLineByNumber(_lineNumber);
        foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, line))
        {
            drawingContext.DrawRectangle(_highlightBrush, null,
                new System.Windows.Rect(rect.X, rect.Y, textView.ActualWidth, rect.Height));
        }
    }
}

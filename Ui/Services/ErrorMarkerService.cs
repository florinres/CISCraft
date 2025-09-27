using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace Ui.Services
{
    /// <summary>
    /// Service for highlighting errors in the code editor with squiggly underlines
    /// </summary>
    public class ErrorMarkerService : IDisposable
    {
        private readonly TextDocument _document;
        private readonly TextSegmentCollection<ErrorMarker> _markers;
        private readonly IBackgroundRenderer _renderer;
        private readonly IVisualLineTransformer _transformer;
        private readonly TextEditor _textEditor;
        private readonly System.Windows.Controls.ToolTip _toolTip;
        private readonly System.Windows.Controls.TextBlock _toolTipTextBlock;

        /// <summary>
        /// Creates a new ErrorMarkerService
        /// </summary>
        /// <param name="textEditor">The text editor to mark errors in</param>
        public ErrorMarkerService(TextEditor textEditor)
        {
            _textEditor = textEditor ?? throw new ArgumentNullException(nameof(textEditor));
            _document = textEditor.Document;
            _markers = new TextSegmentCollection<ErrorMarker>(_document);
            
            _renderer = new ErrorMarkerBackgroundRenderer(_markers);
            _transformer = new ErrorMarkerVisualLineTransformer(_markers);
            
            textEditor.TextArea.TextView.BackgroundRenderers.Add(_renderer);
            textEditor.TextArea.TextView.LineTransformers.Add(_transformer);
            
            // Initialize tooltip components
            _toolTipTextBlock = new System.Windows.Controls.TextBlock {
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 400
            };
            
            _toolTip = new System.Windows.Controls.ToolTip {
                Content = _toolTipTextBlock,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                PlacementTarget = textEditor
            };
            
            // Set up mouse move handler instead of MouseHover which isn't available
            textEditor.TextArea.MouseMove += TextArea_MouseMove;
            textEditor.TextArea.MouseLeave += TextArea_MouseLeave;
        }
        
        // Track the last offset we showed a tooltip for to avoid constant updates
        private int _lastTooltipOffset = -1;
        
        private void TextArea_MouseMove(object sender, MouseEventArgs e)
        {
            try 
            {
                var pos = _textEditor.TextArea.TextView.GetPositionFloor(
                    e.GetPosition(_textEditor.TextArea.TextView) + 
                    _textEditor.TextArea.TextView.ScrollOffset);
                
                if (!pos.HasValue) 
                {
                    HideToolTip();
                    return;
                }
                
                var logicalPos = pos.Value.Location;
                var offset = _document.GetOffset(logicalPos.Line, logicalPos.Column);
                
                // Only update tooltip if we're at a different position
                if (offset != _lastTooltipOffset)
                {
                    _lastTooltipOffset = offset;
                    
                    var markersAtOffset = _markers.FindSegmentsContaining(offset).ToList();
                    if (markersAtOffset.Any())
                    {
                        ShowToolTip(string.Join(Environment.NewLine, markersAtOffset.Select(m => m.Message)));
                    }
                    else
                    {
                        HideToolTip();
                    }
                }
            }
            catch (Exception ex) 
            {
                // Ensure any exception doesn't crash the application
                System.Diagnostics.Debug.WriteLine($"Error in tooltip handling: {ex.Message}");
                HideToolTip();
            }
        }
        
        private void ShowToolTip(string message)
        {
            _toolTipTextBlock.Text = message;
            _toolTip.IsOpen = true;
        }
        
        private void HideToolTip() 
        {
            _toolTip.IsOpen = false;
        }
        
        private void TextArea_MouseLeave(object sender, MouseEventArgs e)
        {
            HideToolTip();
            _lastTooltipOffset = -1;
        }

        /// <summary>
        /// Adds an error marker for the specified location in the text
        /// </summary>
        /// <param name="startLine">The line where the error starts (1-based)</param>
        /// <param name="startColumn">The column where the error starts (1-based)</param>
        /// <param name="endLine">The line where the error ends (1-based)</param>
        /// <param name="endColumn">The column where the error ends (1-based)</param>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The type of error (Error, Warning, Info)</param>
        /// <returns>The created error marker</returns>
        public ErrorMarker Create(int startLine, int startColumn, int endLine, int endColumn, string message, ErrorType errorType = ErrorType.Error)
        {
            // Convert line/column positions to offsets
            int startOffset = _document.GetOffset(startLine, startColumn);
            int endOffset = _document.GetOffset(endLine, endColumn);

            return Create(startOffset, endOffset, message, errorType);
        }

        /// <summary>
        /// Adds an error marker for the specified text segment
        /// </summary>
        /// <param name="startOffset">The start offset in the document</param>
        /// <param name="endOffset">The end offset in the document</param>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The type of error (Error, Warning, Info)</param>
        /// <returns>The created error marker</returns>
        public ErrorMarker Create(int startOffset, int endOffset, string message, ErrorType errorType = ErrorType.Error)
        {
            if (startOffset > endOffset)
                throw new ArgumentException("startOffset must be less than or equal to endOffset");

            var marker = ErrorMarker.FromOffsets(startOffset, endOffset, message, errorType);
            _markers.Add(marker);
            
            // Ensure the error area is redrawn
            _textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
            
            return marker;
        }

        /// <summary>
        /// Removes a specific error marker
        /// </summary>
        /// <param name="marker">The marker to remove</param>
        public void Remove(ErrorMarker marker)
        {
            if (marker == null) return;
            _markers.Remove(marker);
            _textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
        }

        /// <summary>
        /// Removes all error markers
        /// </summary>
        public void Clear()
        {
            _markers.Clear();
            _textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
        }

        public void Dispose()
        {
            // Clean up event handlers
            _textEditor.TextArea.MouseMove -= TextArea_MouseMove;
            _textEditor.TextArea.MouseLeave -= TextArea_MouseLeave;
            
            // Remove tooltip
            _toolTip.IsOpen = false;
            
            // Remove renderers
            _textEditor.TextArea.TextView.BackgroundRenderers.Remove(_renderer);
            _textEditor.TextArea.TextView.LineTransformers.Remove(_transformer);
        }
    }

    /// <summary>
    /// Represents a marked error in the text
    /// </summary>
    public class ErrorMarker : TextSegment
    {
        /// <summary>
        /// The error message
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// The type of error
        /// </summary>
        public ErrorType Type { get; }

        /// <summary>
        /// Creates a new error marker with a start offset and length
        /// </summary>
        /// <param name="startOffset">Start offset in the document</param>
        /// <param name="length">Length of the error text</param>
        /// <param name="message">Error message</param>
        /// <param name="type">Type of error</param>
        public ErrorMarker(int startOffset, int length, string message, ErrorType type)
        {
            StartOffset = startOffset;
            Length = length;
            Message = message;
            Type = type;
        }

        /// <summary>
        /// Creates a new error marker with start and end offsets
        /// </summary>
        /// <param name="startOffset">Start offset in the document</param>
        /// <param name="endOffset">End offset in the document</param>
        /// <param name="message">Error message</param>
        /// <param name="type">Type of error</param>
        public static ErrorMarker FromOffsets(int startOffset, int endOffset, string message, ErrorType type)
        {
            return new ErrorMarker(startOffset, endOffset - startOffset, message, type);
        }
    }

    /// <summary>
    /// The type of error marker
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// An error (red squiggly underline)
        /// </summary>
        Error,
        
        /// <summary>
        /// A warning (yellow squiggly underline)
        /// </summary>
        Warning,
        
        /// <summary>
        /// Informational (blue squiggly underline)
        /// </summary>
        Info
    }

    /// <summary>
    /// Renders the squiggly underlines for error markers
    /// </summary>
    internal class ErrorMarkerBackgroundRenderer : IBackgroundRenderer
    {
        private readonly TextSegmentCollection<ErrorMarker> _markers;

        public ErrorMarkerBackgroundRenderer(TextSegmentCollection<ErrorMarker> markers)
        {
            _markers = markers ?? throw new ArgumentNullException(nameof(markers));
        }

        public KnownLayer Layer => KnownLayer.Selection; // using Selection layer for drawing

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView == null || drawingContext == null) return;

            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0) return;

            var viewStart = visualLines[0].FirstDocumentLine.Offset;
            var viewEnd = visualLines[visualLines.Count - 1].LastDocumentLine.EndOffset;

            foreach (var marker in _markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
            {
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    double waveWidth = 2.5;
                    double waveHeight = 3.0;

                    // Create the squiggly underline
                    var geometry = new StreamGeometry();

                    using (var context = geometry.Open())
                    {
                        context.BeginFigure(rect.BottomLeft, false, false);
                        
                        // Create the squiggle line points
                        var points = new List<Point>();
                        for (double x = rect.Left; x < rect.Right; x += waveWidth)
                        {
                            points.Add(new Point(x, rect.Bottom - waveHeight));
                            points.Add(new Point(Math.Min(x + waveWidth / 2, rect.Right), rect.Bottom));
                        }
                        
                        context.PolyLineTo(points, true, false);
                    }

                    geometry.Freeze();

                    Brush brush;
                    switch (marker.Type)
                    {
                        case ErrorType.Warning:
                            brush = Brushes.Gold;
                            break;
                        case ErrorType.Info:
                            brush = Brushes.DeepSkyBlue;
                            break;
                        case ErrorType.Error:
                        default:
                            brush = Brushes.Red;
                            break;
                    }

                    drawingContext.DrawGeometry(null, new Pen(brush, 1), geometry);
                }
            }
        }
    }

    /// <summary>
    /// Line transformer that sets tooltips for error markers
    /// </summary>
    internal class ErrorMarkerVisualLineTransformer : IVisualLineTransformer
    {
        private readonly TextSegmentCollection<ErrorMarker> _markers;

        public ErrorMarkerVisualLineTransformer(TextSegmentCollection<ErrorMarker> markers)
        {
            _markers = markers ?? throw new ArgumentNullException(nameof(markers));
        }

        public void Transform(ITextRunConstructionContext context, IList<VisualLineElement> elements)
        {
            if (context == null || elements == null) return;

            // We don't modify the elements at all, we just need a line transformer to install tool tips
            var line = context.VisualLine;
            if (line == null) return;

            // In AvalonEdit, tooltips need to be handled differently
            // Instead of setting directly on elements, we'll use mouse hover events
            // This part is now handled by the MouseHoverLogic in the ErrorMarkerService
        }
    }
}
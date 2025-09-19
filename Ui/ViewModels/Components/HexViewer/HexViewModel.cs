using MainMemory.Business;
using MainMemory.Business.Models;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Models;
using Ui.ViewModels.Components.MenuBar;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Components.HexViewer;

public partial class HexViewModel : ToolViewModel, IHexViewModel
{
    [ObservableProperty] public override partial string? Title { get; set; } = "MainMemory";
    [ObservableProperty]
    public partial string DataStringVisual { get; set; } = "Hexadecimal";
    private readonly IAssemblerService _assemblerService;
    private readonly MemoryContentWrapper _memoryContentWrapper;
    private readonly IMainMemory _mainMemory;

    public HexViewModel(IAssemblerService assemblerService, MemoryContentWrapper memoryContentWrapper, IMainMemory mainMemory)
    {
        _assemblerService = assemblerService;
        _mainMemory = mainMemory;
        _memoryContentWrapper = memoryContentWrapper;

        // Subscribe to the event
        _assemblerService.SourceCodeAssembled += OnSourceCodeAssembled;
        _memoryContentWrapper.PropertyChanged += OnMemoryChanged;

    }

    [ObservableProperty]
    private byte[] _assembledCode = [];

    [ObservableProperty]
    private bool _isElementReadyToRender;

    [ObservableProperty]
    private Stream _hexEditorStream = new MemoryStream();

    private void OnSourceCodeAssembled(object? sender, byte[] code)
    {
        AssembledCode = code;
        // Don't update HexEditorStream here, let OnAssembledCodeChanged handle it
        IsElementReadyToRender = AssembledCode is { Length: > 0 };
        // Don't call RefreshHexViewFromMemory() here to avoid double refresh
    }

    partial void OnAssembledCodeChanged(byte[]? oldValue, byte[] newValue)
    {
        // Use MemoryContentWrapper directly rather than creating a new stream from the assembled code
        RefreshHexViewFromMemory();
    }
    private void OnMemoryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null || e.PropertyName == nameof(_memoryContentWrapper.MemoryContent))
        {
            RefreshHexViewFromMemory();
            return;
        }

        var index = TryParseIndexFromPropertyName(e.PropertyName);
        RefreshHexViewFromMemory(index);
    }

    private static int? TryParseIndexFromPropertyName(string name)
    {
        var match = Regex.Match(name, @"Memory\[(\d+)\]");
        if (int.TryParse(match.Groups[1].Value, out var index))
        {
            return index;
        }
        return null;
    }

    private void RefreshHexViewFromMemory(int? index = null)
    {
        // If we're just updating a single byte, try to do it without recreating the stream
        if (HexEditorStream.CanWrite && index is not null)
        {
            try
            {
                HexEditorStream.Position = index.Value;
                HexEditorStream.WriteByte(_memoryContentWrapper[index.Value]);
                HexEditorStream.Position = 0; // Reset position
                return; // Exit early to avoid creating a new stream
            }
            catch (Exception)
            {
                // If updating a single byte fails, fall back to recreating the stream
            }
        }

        // Only dispose and recreate the stream if necessary
        using (var oldStream = HexEditorStream)
        {
            // Create new stream
            var newStream = new MemoryStream(_memoryContentWrapper.MemoryContent, writable: true);
            
            // Assign new stream first before potentially disposing the old one
            // This ensures we never have a null stream
            HexEditorStream = newStream;
        }
        
        IsElementReadyToRender = true;
    }
    public void SetNumberFormat(NumberFormat format)
    {
        switch (format)
        {
            case NumberFormat.Hex:
                DataStringVisual = "Hexadecimal";
                break;
            case NumberFormat.Decimal:
                DataStringVisual = "Decimal";
                break;
            case NumberFormat.Binary:
                DataStringVisual = "Binary";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }
}
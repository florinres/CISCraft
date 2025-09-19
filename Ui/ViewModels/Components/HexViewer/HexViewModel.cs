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
    [ObservableProperty] public override partial string? Title { get; set; } = "HexViewer";
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
        HexEditorStream = new MemoryStream(AssembledCode, writable: false);
        IsElementReadyToRender = AssembledCode is { Length: > 0 };
        RefreshHexViewFromMemory();
    }

    partial void OnAssembledCodeChanged(byte[]? oldValue, byte[] newValue)
    {
        HexEditorStream = new MemoryStream(newValue, 0, newValue.Length, false);
        IsElementReadyToRender = newValue is { Length: > 0 };
    }
    private void OnMemoryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null)
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
        if (HexEditorStream.CanSeek && index is not null)
        {
            HexEditorStream.Position = index.Value;
            HexEditorStream.WriteByte(_memoryContentWrapper[index.Value]);
            HexEditorStream.Position = index.Value;
        }

        HexEditorStream?.Dispose();
        HexEditorStream = new MemoryStream(_memoryContentWrapper.MemoryContent, writable: true);
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
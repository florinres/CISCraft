using System.ComponentModel;
using System.IO;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Components.HexViewer;

public partial class HexViewModel : ToolViewModel, IHexViewModel
{
    [ObservableProperty] public override partial string? Title { get; set; } = "HexViewer";
    private readonly IAssemblerService _assemblerService;

    public HexViewModel(IAssemblerService assemblerService)
    {
        _assemblerService = assemblerService;

        // Subscribe to the event
        _assemblerService.SourceCodeAssembled += OnSourceCodeAssembled;
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
    }

    partial void OnAssembledCodeChanged(byte[]? oldValue, byte[] newValue)
    {
        HexEditorStream = new MemoryStream(newValue, writable: false);
        IsElementReadyToRender = newValue is { Length: > 0 };
    }
}
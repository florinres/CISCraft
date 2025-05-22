using System.Buffers.Binary;
using System.ComponentModel;
using System.IO;
using MainMemory.Business.Models;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Components.HexViewer;

public partial class HexViewModel : ToolViewModel, IHexViewModel
{
    [ObservableProperty] public override partial string? Title { get; set; } = "HexViewer";
    private readonly IAssemblerService _assemblerService;
    private readonly MomeryContentWrapper _memoryContentWrapper;

    public HexViewModel(IAssemblerService assemblerService, MomeryContentWrapper momeryContentWrapper)
    {
        _assemblerService = assemblerService;
        _memoryContentWrapper = momeryContentWrapper;

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
        HexEditorStream = new MemoryStream(newValue, writable: false);
        IsElementReadyToRender = newValue is { Length: > 0 };
    }
    private void OnMemoryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName?.StartsWith("Memory[") == true)
        {
            RefreshHexViewFromMemory();
        }
    }

    private void RefreshHexViewFromMemory()
    {
        HexEditorStream = new MemoryStream(_memoryContentWrapper.MemoryContent, writable: false);
        IsElementReadyToRender = _memoryContentWrapper.Length > 0;
    }


    
}
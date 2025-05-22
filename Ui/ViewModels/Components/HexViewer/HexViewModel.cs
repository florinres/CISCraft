using System.Buffers.Binary;
using System.ComponentModel;
using System.IO;
using MainMemory.Business;
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
    private readonly IMainMemory _mainMemory;

    public HexViewModel(IAssemblerService assemblerService, MomeryContentWrapper momeryContentWrapper, IMainMemory mainMemory)
    {
        _assemblerService = assemblerService;
        _mainMemory = mainMemory;
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
        HexEditorStream = new MemoryStream(newValue, 0, newValue.Length, false);
        IsElementReadyToRender = newValue is { Length: > 0 };
    }
    private void OnMemoryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName?.StartsWith("Memory[") == true)
        {
            RefreshHexViewFromMemory();
        }
    }

    //TODO: clean up this and make it index specific
    private void RefreshHexViewFromMemory()
    {
        int lengthToCopy = Math.Min(_mainMemory.memoryLocationsNum, _memoryContentWrapper.MemoryContent.Length);

        // Assume 4-byte aligned and data represents int32s
        int intCount = lengthToCopy / 4;
        byte[] limitedBuffer = new byte[intCount * 4];

        for (int i = 0; i < intCount; i++)
        {
            int value = BitConverter.ToInt32(_memoryContentWrapper.MemoryContent, i * 4);

            // Force little endian regardless of system endianness
            byte[] leBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(leBytes);

            Array.Copy(leBytes, 0, limitedBuffer, i * 4, 4);
        }

        HexEditorStream?.Dispose(); // Dispose the old stream if needed
        HexEditorStream = new MemoryStream(limitedBuffer, writable: false);
        HexEditorStream.Position = 0;

        IsElementReadyToRender = lengthToCopy > 0;
    }


    
}
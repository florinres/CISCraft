using System.ComponentModel;
using Ui.ViewModels.Components.HexViewer;

namespace Ui.Interfaces.ViewModel;

public interface IHexViewModel : IToolViewModel
{
    /// <inheritdoc cref="HexViewModel._assembledCode"/>
    byte[] AssembledCode
    {
        get;
        set;
    }

    /// <inheritdoc cref="HexViewModel._isElementReadyToRender"/>
    bool IsElementReadyToRender { get; set; }

    /// <inheritdoc cref="HexViewModel._hexEditorStream"/>
    System.IO.Stream? HexEditorStream { get; set; }
}
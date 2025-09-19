using System.ComponentModel;
using Ui.Models;
using Ui.ViewModels.Components.HexViewer;
using Ui.ViewModels.Components.MenuBar;

namespace Ui.Interfaces.ViewModel;

public interface IHexViewModel : IToolViewModel
{
    /// <inheritdoc cref="HexViewModel._assembledCode"/>
    byte[] AssembledCode
    {
        get;
        set;
    }
    string DataStringVisual { get; set; }
    public void SetNumberFormat(NumberFormat format);

    /// <inheritdoc cref="HexViewModel._isElementReadyToRender"/>
    bool IsElementReadyToRender { get; set; }

    /// <inheritdoc cref="HexViewModel._hexEditorStream"/>
    System.IO.Stream? HexEditorStream { get; set; }
}
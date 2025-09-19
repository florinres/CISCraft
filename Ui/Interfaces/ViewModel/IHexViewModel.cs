using System.ComponentModel;
using System.IO;
using System.Windows.Input;
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
    Stream? HexEditorStream { get; set; }
    
    /// <summary>
    /// Command to navigate to a specific memory address
    /// </summary>
    ICommand GotoAddressCommand { get; }
    
    /// <summary>
    /// Navigate to a specific memory address
    /// </summary>
    /// <param name="address">Address to navigate to</param>
    /// <returns>True if navigation was successful</returns>
    bool GotoAddress(long address);
}
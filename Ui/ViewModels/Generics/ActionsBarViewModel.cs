using System.Text;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;

namespace Ui.ViewModels.Generics;

public partial class ActionsBarViewModel : ObservableObject, IActionsBarViewModel
{
    private readonly IActiveDocumentService _activeDocumentService;
    private readonly IAssemblerService _assemblerService;

    public ActionsBarViewModel(IAssemblerService assemblerService, IActiveDocumentService activeDocumentService)
    {
        _assemblerService = assemblerService;
        _activeDocumentService = activeDocumentService;

        ObjectCodeGenerated += OnObjectCodeGenerated;
    }

    public event EventHandler<byte[]>? ObjectCodeGenerated;

    [RelayCommand]
    private void RunAssembleSourceCodeService()
    {
        if (_activeDocumentService.SelectedDocument == null) return;
        var objectCode = _assemblerService.AssembleSourceCodeService(_activeDocumentService.SelectedDocument.Content);
        ObjectCodeGenerated?.Invoke(this, objectCode);
    }

    private void OnObjectCodeGenerated(object? sender, byte[] objectCode)
    {
        if (objectCode is not [])
        {
            _activeDocumentService.HexViewer.IsVisible = true;
        }
        
        //
        // var result = Encoding.Unicode.GetString(objectCode);
        // var doc = new FileViewModel
        // {
        //     Title = "Assembled File",
        //     Content = result
        // };
        // _activeDocumentService.Documents.Add(doc);
        // _activeDocumentService.SelectedDocument ??= doc;
    }
}
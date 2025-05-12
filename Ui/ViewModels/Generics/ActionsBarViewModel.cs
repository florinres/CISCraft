using System.Text;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;

namespace Ui.ViewModels.Generics;

public partial class ActionsBarViewModel : ObservableObject, IActionsBarViewModel
{
    private readonly IActiveDocumentService _activeDocumentService;
    private readonly IAssemblerService _assemblerService;
    private readonly IDockingService _dockingService;

    public ActionsBarViewModel(IAssemblerService assemblerService, IActiveDocumentService activeDocumentService ,IDockingService dockingService)
    {
        _assemblerService = assemblerService;
        _activeDocumentService = activeDocumentService;
        _dockingService = dockingService;

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
            _dockingService.ShowTool(_activeDocumentService.HexViewer);
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
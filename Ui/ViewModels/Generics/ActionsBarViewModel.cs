using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.Interfaces.Windows;
using ASMBLR = Assembler.Business.Assembler;

namespace Ui.ViewModels.Generics;

public partial class ActionsBarViewModel : ObservableObject, IActionsBarViewModel
{
    public event EventHandler<byte[]>? ObjectCodeGenerated;
    private readonly IAssemblerService _assemblerService;
    private readonly IActiveDocumentService _activeDocumentService;

    public ActionsBarViewModel(IAssemblerService assemblerService,IActiveDocumentService activeDocumentService)
    {
        _assemblerService = assemblerService;
        _activeDocumentService = activeDocumentService;

        ObjectCodeGenerated += OnObjectCodeGenerated;
    }
    [RelayCommand]
    private void RunAssembleSourceCodeService()
    {
        if (_activeDocumentService.SelectedDocument == null)
        {
            return;
        }
        var objectCode = _assemblerService.AssembleSourceCodeService(_activeDocumentService.SelectedDocument.Content);
        ObjectCodeGenerated?.Invoke(this, objectCode);
    }
    
    private void OnObjectCodeGenerated(object? sender, byte[] objectCode)
    {
        string result = Encoding.Unicode.GetString(objectCode);
        var doc = new FileViewModel()
        {
            Title = "Assembled File",
            Content = result,
        };
        _activeDocumentService.Documents.Add(doc);
        _activeDocumentService.SelectedDocument ??= doc;
    }
}

using System;
using System.Collections.Generic;
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
    private readonly IActiveDocumentService _activeDocument;

    public ActionsBarViewModel(IAssemblerService assemblerService,IActiveDocumentService activeDocument)
    {
        _assemblerService = assemblerService;
        _activeDocument = activeDocument;
    }
    [RelayCommand]
    private void RunAssembleSourceCodeService()
    {
        if (_activeDocument.SelectedDocument == null)
        {
            return;
        }
        byte[] objectCode = _assemblerService.AssembleSourceCodeService(_activeDocument.SelectedDocument.Content);
        ObjectCodeGenerated?.Invoke(this, objectCode);
    }
}

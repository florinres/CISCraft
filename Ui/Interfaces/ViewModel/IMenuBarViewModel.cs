using System;
using System.Collections.ObjectModel;
using Ui.ViewModels;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.ViewModel;
public interface IMenuBarViewModel
{
    ObservableCollection<FileViewModel> Documents { get; }
    ObservableCollection<ToolViewModel> Tools { get; }
    FileViewModel? SelectedDocument { get; set; }
    FileStatsViewModel FileStats
    {
        get;
        [System.Diagnostics.CodeAnalysis.MemberNotNull("_fileStats")]
        set;
    }
}

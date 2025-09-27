using System.Collections.ObjectModel;
using System.ComponentModel;
using Ui.Models;
using Ui.ViewModels.Components.Diagram;

namespace Ui.Interfaces.ViewModel;

public interface IMicroprogramViewModel : IToolViewModel
{
    ObservableCollection<MicroprogramMemoryViewModel> Rows { get; set; }

    NumberFormat AddressFormat { get; set; }
    public MicroprogramMemoryViewModel CurrentMemoryRow { get; set; }
    int CurrentRow { get; set; }

    int CurrentColumn { get; set; }
    void LoadMicroprogramFromJson(string json);
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
    void ClearAllHighlightedRows();
    void ClearAllHighlight();
    int SearchForLabel(string label);
}
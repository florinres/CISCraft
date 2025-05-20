using System.Collections.ObjectModel;
using System.Text.Json;
using Ui.Interfaces.ViewModel;
using Ui.Models;
using Ui.ViewModels.Components.Diagram;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Components.Microprogram;

public partial class MicroprogramViewModel : ToolViewModel, IMicroprogramViewModel
{
    [ObservableProperty]
    public partial ObservableCollection<MicroprogramMemoryViewModel> Rows { get; set; } = [];

    [ObservableProperty] public partial NumberFormat AddressFormat { get; set; } = NumberFormat.Hex;
    
    [ObservableProperty] public partial int CurrentRow { get; set; } = 0;
    partial void OnCurrentRowChanged(int oldValue, int newValue)
    {
        if (oldValue >= 0 && oldValue < Rows.Count)
        {
            Rows[oldValue].IsCurrent = false;
            Rows[oldValue][CurrentColumn].IsCurrent = false;
        }

        if (newValue >= 0 && newValue < Rows.Count)
        {
            Rows[newValue].IsCurrent = true;
            Rows[oldValue][CurrentColumn].IsCurrent = true;
        }
    }
    
    partial void OnCurrentColumnChanged(int oldValue, int newValue)
    {
        if (oldValue >= 0 && oldValue < Rows.Count)
        {
            Rows[CurrentRow][oldValue].IsCurrent = false;
        }

        if (newValue >= 0 && newValue < Rows.Count)
            Rows[CurrentRow][newValue].IsCurrent = true;
    }

    [ObservableProperty] public partial int CurrentColumn { get; set; } = 0;

    public void LoadMicroprogramFromJson(string json)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, List<List<string>>>>(json);
        //TODO: handle null case
        var flatMatrix = dict!.Values.SelectMany(x => x).ToList();

        Rows = new ObservableCollection<MicroprogramMemoryViewModel>(
            flatMatrix.Select((row, index) => new MicroprogramMemoryViewModel
            {
                Items = new ObservableCollection<MicroInstructionItem>(
                    row.Select(r => new MicroInstructionItem
                    {
                        Value = r,
                        IsCurrent = false
                    })
                ),
                Address = index
            })
        );
        ;

    }

    public override string? Title { get; set; } = "Microprogram";
}
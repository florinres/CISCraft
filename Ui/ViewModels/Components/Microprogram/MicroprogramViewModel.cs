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
    [ObservableProperty] public partial int CurrentColumn { get; set; } = 0;

    public void LoadMicroprogramFromJson(string json)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, List<List<string>>>>(json);
        //TODO: handle null case
        var flatMatrix = dict!.Values.SelectMany(x => x).ToList();

        Rows = new ObservableCollection<MicroprogramMemoryViewModel>(
            flatMatrix.Select((row, index) => new MicroprogramMemoryViewModel
            {
                Item0 = row.ElementAtOrDefault(0) ?? "",
                Item1 = row.ElementAtOrDefault(1) ?? "",
                Item2 = row.ElementAtOrDefault(2) ?? "",
                Item3 = row.ElementAtOrDefault(3) ?? "",
                Item4 = row.ElementAtOrDefault(4) ?? "",
                Item5 = row.ElementAtOrDefault(5) ?? "",
                Item6 = row.ElementAtOrDefault(6) ?? "",
                Item7 = row.ElementAtOrDefault(7) ?? "",
                Item8 = row.ElementAtOrDefault(8) ?? "",
                Item9 = row.ElementAtOrDefault(9) ?? "",
                Address = index
            })
        );

    }

    public override string? Title { get; set; } = "Microprogram";
}
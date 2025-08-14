using System.Collections.ObjectModel;
using System.Text.Json;
using Ui.Interfaces.ViewModel;
using Ui.Models;
using Ui.ViewModels.Components.Diagram;
using Ui.ViewModels.Generics;

namespace Ui.ViewModels.Components.Microprogram;

public partial class MicroprogramViewModel : ToolViewModel, IMicroprogramViewModel
{
    public MicroprogramViewModel()
    {
        ZoomFactor = 12;
    }
    
    [ObservableProperty]
    public partial ObservableCollection<MicroprogramMemoryViewModel> Rows { get; set; } = [];
    
    [ObservableProperty]
    public partial MicroprogramMemoryViewModel? CurrentMemoryRow { get; set; }

    [ObservableProperty] public partial NumberFormat AddressFormat { get; set; } = NumberFormat.Hex;
    
    [ObservableProperty] public partial int CurrentRow { get; set; } = -1;
    partial void OnCurrentRowChanged(int oldValue, int newValue)
    {
        if (oldValue >= 0 && oldValue < Rows.Count)
        {
            //Rows[oldValue].IsCurrent = false;
            foreach (var item in Rows[oldValue].Items)
            {
                item.IsCurrent = false;
            }
        }

        // Set new row highlight
        if (newValue >= 0 && newValue < Rows.Count)
        {
            Rows[newValue].IsCurrent = true;

            // Optional: highlight the current column in new row if valid
            if (CurrentColumn >= 0 && CurrentColumn < Rows[newValue].Items.Count)
            {
                Rows[newValue].Items[CurrentColumn].IsCurrent = true;
            }
            
            CurrentMemoryRow = Rows[newValue];
        }
    }

    public void ClearAllHighlightedRows()
    {
        foreach (var row in Rows.Where(r => r.IsCurrent))
        {
            row.IsCurrent = false;
        }
    }
    
    public void ClearAllHighlight()
    {
        foreach (var row in Rows)
        {
            foreach (var item in row.Items)
            {
                item.IsCurrent = false;
            }
            row.IsCurrent = false;
        }
    }

    partial void OnCurrentColumnChanged(int oldValue, int newValue)
    {
        // Clear old cell highlight
        if (CurrentRow >= 0 && CurrentRow < Rows.Count)
        {
            var row = Rows[CurrentRow];

            if (oldValue >= 0 && oldValue < row.Items.Count)
                row.Items[oldValue].IsCurrent = false;

            if (newValue >= 0 && newValue < row.Items.Count)
                row.Items[newValue].IsCurrent = true;
        }
    }


    [ObservableProperty] public partial int CurrentColumn { get; set; } = -1;

    public void LoadMicroprogramFromJson(string json)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, List<List<string>>>>(json);
        var rows = new List<MicroprogramMemoryViewModel>();
        int index = 0;

        foreach (var kvp in dict!)
        {
            bool isFirst = true;

            foreach (var row in kvp.Value)
            {
                rows.Add(new MicroprogramMemoryViewModel
                {
                    Items = new ObservableCollection<MicroInstructionItem>(
                        row.Select(r => new MicroInstructionItem
                        {
                            Value = r,
                            IsCurrent = false,
                        })
                    ),
                    Address = index++,
                    Tag = isFirst ? kvp.Key : "" // Only tag the first item of each group
                });

                isFirst = false;
            }
        }
        Rows = new ObservableCollection<MicroprogramMemoryViewModel>(rows);
    }

    public override string? Title { get; set; } = "Microprogram";
}
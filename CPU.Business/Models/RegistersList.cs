using CommunityToolkit.Mvvm.ComponentModel;

namespace CPU.Business.Models;

public class RegistersList : ObservableObject
{
    private short[] values { get; } = [];
    
    public short this[int index]
    {
        get => values[index];
        set
        {
            if (values[index] == value) return;
            
            values[index] = value;
            OnPropertyChanged($"Item[{index}]");
            OnPropertyChanged(nameof(Values));
        }
    }
    
    public short[] Values => values;
}
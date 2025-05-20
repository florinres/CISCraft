using CommunityToolkit.Mvvm.ComponentModel;

namespace CPU.Business.Models;

public class RegistersList : ObservableObject
{
    private short[] values { get; }

    public RegistersList(int amount)
    {
        values = new short[amount];
    }
    public RegistersList()
    {
        values = new short[23];
    }
    
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
    
    public short this[REGISTERS index]
    {
        get => values[(int)index];
        set
        {
            if (values[(int)index] == value) return;
            
            values[(int)index] = value;
            OnPropertyChanged($"Item[{index}]");
            OnPropertyChanged(nameof(Values));
        }
    }
    public short[] Values => values;
}
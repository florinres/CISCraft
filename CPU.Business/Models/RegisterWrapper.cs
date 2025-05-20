using CommunityToolkit.Mvvm.ComponentModel;

namespace CPU.Business.Models;

public class RegisterWrapper : ObservableObject
{
    private short[] _registers { get; }
    private short[] _gpr { get; }

    public RegisterWrapper(int amount)
    {
        _registers = new short[amount];
        _gpr = new short[16];
    }
    public RegisterWrapper()
    {
        _registers = new short[23];
        _gpr = new short[16];
    }

    public short this[int index]
    {
        get => _registers[index];
        set
        {
            if (_registers[index] == value) return;
            
            _registers[index] = value;
            OnPropertyChanged($"Item[{index}]");
            OnPropertyChanged(nameof(_registers));
        }
    }
    
    public short this[REGISTERS index]
    {
        get => _registers[(int)index];
        set
        {
            if (_registers[(int)index] == value) return;
            
            _registers[(int)index] = value;
            OnPropertyChanged($"Item[{index}]");
            OnPropertyChanged(nameof(_registers));
        }
    }
    
    public short this[GPR index]
    {
        get => _registers[(int)index];
        set
        {
            if (_registers[(int)index] == value) return;
            
            _registers[(int)index] = value;
            OnPropertyChanged($"Item[{index}]");
            OnPropertyChanged(nameof(_registers));
        }
    }
    public short[] Registers => _registers;
    public short[] Gpr => _gpr;
}
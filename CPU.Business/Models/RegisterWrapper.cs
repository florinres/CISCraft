using CommunityToolkit.Mvvm.ComponentModel;

namespace CPU.Business.Models;

public class RegisterWrapper(int amount = 23) : ObservableObject
{
    private short[] _registers { get; } = new short[amount];
    private short[] _gpr { get; } = new short[16];

    private byte _mar = 0;

    public byte MAR
    {
        get => _mar;
        set
        {
            if (_mar == value) return;
            
            OnPropertyChanged($"{nameof(MAR)}");
        }
    }
    
    private long _mir = 0;

    public long MIR
    {
        get => _mir;
        set
        {
            if (_mir == value) return;
            
            OnPropertyChanged($"{nameof(MIR)}");
        }
    }

    public short this[REGISTERS index]
    {
        get => _registers[(int)index];
        set
        {
            if (_registers[(int)index] == value) return;

            _registers[(int)index] = value;
            OnPropertyChanged($"Registers[{index}]");
            OnPropertyChanged(nameof(_registers));
        }
    }

    public short this[GPR index]
    {
        get => _gpr[(int)index];
        set
        {
            if (_gpr[(int)index] == value) return;

            _gpr[(int)index] = value;
            OnPropertyChanged($"Gpr[{index}]");
            OnPropertyChanged(nameof(_gpr));
        }
    }
    public short[] Registers => _registers;
    public short[] Gpr => _gpr;
}
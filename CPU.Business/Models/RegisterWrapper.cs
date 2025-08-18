using CommunityToolkit.Mvvm.ComponentModel;

namespace CPU.Business.Models;

public class RegisterWrapper : ObservableObject
{
    private short[] _registers { get; }
    private short[] _gpr { get; }
    private bool[] _irqs { get; }
    private bool[] _exceptions { get; }

    public RegisterWrapper(int amount)
    {
        _registers = new short[amount];
        _gpr = new short[16];
        _irqs = new bool[4];
        _exceptions = new bool[4];
    }
    public RegisterWrapper()
    {
        _registers = new short[23];
        _gpr = new short[16];
        _irqs = new bool[4];
        _exceptions = new bool[4];
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
    public bool this[IRQs index]
    {
        get => _irqs[(int)index];
        set
        {
            if (_irqs[(int)index] == value) return;

            _irqs[(int)index] = value;
            OnPropertyChanged($"Registers[{_irqs}]");
            OnPropertyChanged(nameof(_irqs));
        }
    }
    public bool this[Exceptions index]
    {
        get => _exceptions[(int)index];
        set
        {
            if (_exceptions[(int)index] == value) return;

            _irqs[(int)index] = value;
            OnPropertyChanged($"Registers[{_exceptions}]");
            OnPropertyChanged(nameof(_exceptions));
        }
    }
    public short[] Registers => _registers;
    public short[] Gpr => _gpr;
    public bool[] Irqs => _irqs;
    public bool[] Exceptions => _exceptions;
}
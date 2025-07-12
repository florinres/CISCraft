using System.Buffers.Binary;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MainMemory.Business.Models;

public class MomeryContentWrapper : ObservableObject
{
    private byte[] _memoryContent { get; }

    public MomeryContentWrapper(int size)
    {
        _memoryContent = new byte[size];
    }
    
    public MomeryContentWrapper()
    {
        _memoryContent = new byte[1 << 16];
    }

    private bool _notifyChange = true;

    public bool NotifyChange { get => _notifyChange;
        set
        {
            if (value)
            {
                OnPropertyChanged("Memory[0]");
                OnPropertyChanged(nameof(_memoryContent));
            }

            _notifyChange = value;
        } 
    }

    public byte this[int index]
    {
        get => _memoryContent[index];
        set
        {
            if (_memoryContent[index] == value) return;

            _memoryContent[index] = value;
            
            if (!NotifyChange) return;
            
            OnPropertyChanged($"Memory[{index}]");
            OnPropertyChanged(nameof(_memoryContent));
        }
    }
    
    public ushort GetUInt16BigEndian(int offset)
    {
        return BinaryPrimitives.ReadUInt16BigEndian(_memoryContent.AsSpan(offset));
    }

    public void SetUInt16BigEndian(int offset, ushort value)
    {
        BinaryPrimitives.WriteUInt16BigEndian(_memoryContent.AsSpan(offset), value);

        // Trigger change notifications for affected bytes
        OnPropertyChanged($"Memory[{offset}]");
        OnPropertyChanged($"Memory[{offset + 1}]");
    }


    public byte[] MemoryContent => _memoryContent;

    public int Length => _memoryContent.Length;

}
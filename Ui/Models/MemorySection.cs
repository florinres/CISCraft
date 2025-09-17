using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Models
{
    public class MemorySection
    {
        public string Name;
        public ushort StartAddress;
        public ushort EndAddress;
        public string Code;
        public Dictionary<short, ushort> DebugSymbols;
        public MemorySection(string name, ushort startAddress, ushort endAddress, Dictionary<short, ushort> debugSymbols, string code)
        {
            Name = name;
            StartAddress = startAddress;
            EndAddress = endAddress;
            DebugSymbols = debugSymbols;
            Code = code;
        }
    }
}

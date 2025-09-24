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
        public Dictionary<ushort, ushort> DebugSymbols;
        public MemorySection()
        {
            Name = "";
            StartAddress = 0;
            EndAddress = 0;
            DebugSymbols = new Dictionary<ushort, ushort>();
            Code = "";
        }
        public MemorySection(string name, ushort startAddress, ushort endAddress, Dictionary<ushort, ushort> debugSymbols, string code)
        {
            Name = name;
            StartAddress = startAddress;
            EndAddress = endAddress;
            DebugSymbols = debugSymbols;
            Code = code;
        }
    }
}

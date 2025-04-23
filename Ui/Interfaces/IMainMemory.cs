using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Interfaces
{
    interface IMainMemory
    {
        public void SetStackSize(int stackSize);
        public void SetMemoryLocationData(int memoryAddress, byte content);
        public void SetStackPointer(int stackAddress);
        public byte GetMemoryLocationData(int memoryAddress);
        public int GetStackPointer();
        public byte[] GetMemoryDump();
        public void LoadMachineCode(byte[] machineCode);
        public void ClearMemory();
    }
}

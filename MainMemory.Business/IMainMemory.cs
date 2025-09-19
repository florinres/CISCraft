using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainMemory.Business
{
    public interface IMainMemory
    {
        public void SetByteLocation(ushort memoryAddress, byte content);
        public void SetWordLocation(ushort memoryAddress, ushort content);
        public void SetISR(ushort handlerAddress, byte[] interruptRoutine);
        public byte FetchByte(ushort memoryAddress);
        public ushort FetchWord(ushort address);
        public byte[] GetMemoryDump();
        public void LoadMachineCode(byte[] machineCode);
        public void LoadAtOffset(byte[] machineCode, ushort sectionOffset);
        public void ClearMemory();
    }
}

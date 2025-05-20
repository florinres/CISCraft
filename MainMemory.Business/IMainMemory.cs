using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainMemory.Business
{
    public interface IMainMemory
    {
        public void SetStackSize(int stackSize);
        public void SetByteLocation(int memoryAddress, byte content);
        public void SetWordLocation(int memoryAddress, short content);
        public void SetISR(int interruptNumber, byte[] interruptRoutine);
        public byte FetchByte(int memoryAddress);
        public short FetchWord(int address);
        public byte[] GetMemoryDump();
        public void LoadMachineCode(byte[] machineCode);
        public void ClearISR(int interruptNumber);
        public void ClearMemory();
    }
}

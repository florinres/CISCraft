using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Interfaces;
using MainMemoryModule = MainMemory.Business.MainMemory;

namespace Ui.Services
{
    class MainMemoryService : IMainMemory
    {
        readonly MainMemoryModule RAM;

        public MainMemoryService()
        {
            this.RAM = MainMemoryModule.GetMainMemoryInstance();
        }

        public void SetStackSize(int stackSize)
        {
           if(stackSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(stackSize), "Stack size must be a positive number! Please try another value.");

            this.RAM.SetInternalStackSize(stackSize);

        }

        public void SetMemoryLocationData(int memoryAddress, byte content)
        {
            if(memoryAddress < 0)
                throw new ArgumentOutOfRangeException(nameof(memoryAddress), "Memory adress must be a positive number! Please try another value.");

        }

        public void LoadMachineCode(byte[] machineCode)
        {
            if( machineCode == null)
                throw new ArgumentNullException(nameof(machineCode), "Machine code cannot be null.");

            this.RAM.SetInternalMachineCode(machineCode);
        }

        public byte[] GetMemoryDump()
        {
            return this.RAM.GetInternalMemoryDump();
        }

        public byte GetMemoryLocationData(int memoryAddress)
        {
            if (memoryAddress < 0)
                throw new ArgumentOutOfRangeException(nameof(memoryAddress), "Memory adress must be a positive number! Please try another value.");

            return this.RAM.GetInternalLocationData(memoryAddress);
        }
        public void ClearMemory()
        {
            this.RAM.CleanInternalMemory();
        }

        public void SetISR(int interruptNumber, byte[] interruptRoutine)
        {
            if(interruptNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(interruptNumber), "Interrupt number must be a positive number! Please try another value.");

            this.RAM.SetInternalISR(interruptNumber, interruptRoutine);
        }

        public void ClearISR(int interruptNumber)
        {
            if (interruptNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(interruptNumber), "Interrupt number must be a positive number! Please try another value.");

            this.RAM.ClearInternlISR(interruptNumber);
        }
    }
}

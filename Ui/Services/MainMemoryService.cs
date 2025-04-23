using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Interfaces;

namespace Ui.Services
{
    class MainMemoryService : IMainMemory
    {
        public void SetStackSize(int stackSize)
        {

        }

        public void SetMemoryLocationData(int memoryAddress, byte content)
        {

        }

        public void LoadMachineCode(List<byte> machineCode)
        {

        }

        public List<byte> GetMemoryDump()
        {

            return new List<byte>();
        }
    }
}

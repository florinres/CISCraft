using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ui.Interfaces;
using MicroMemory = MicroprogramMemory.Business.MicroProgramMemory;

namespace Ui.Services
{
    class MicroProgramMemoryService : IMicroProgramMemory
    {
        readonly MicroMemory uMemory;
        string microMemoryFile;

        public MicroProgramMemoryService()
        {
            this.uMemory = MicroMemory.GetMicroMemoryInstance();
            this.microMemoryFile = Path.Combine(AppContext.BaseDirectory, "microcode.json");
        }
        public void EditMicroRoutine()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, ulong> GetMicroProgramMemory()
        {
            throw new NotImplementedException();
        }
        public void LoadMicroProgram()
        {
            throw new NotImplementedException();
        }
    }
}

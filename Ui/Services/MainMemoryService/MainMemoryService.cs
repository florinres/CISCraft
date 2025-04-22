using IMainMemory = Ui.Interfaces.IMainMemory.IMainMemory;

namespace Ui.Services.MainMemoryService
{

    public class MainMemoryService : IMainMemory
    {
        public void SetStackSize(int stackSize){

            //if(stackSize<0)
        }

        public void SetMemoryLocationData(int memoryAddress, byte content){

        }

        public void LoadMachineCode(List<byte> machineCode){

        }

        public List<byte> GetMemoryDump(){

            return new List<byte>();
        }
    }
}
namespace Ui.Interfaces.IMainMemory
{

    public class MainMemoryService : IMainMemory
    {

        public void SetStackSize(int stackSize){

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
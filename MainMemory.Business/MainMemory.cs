
namespace MainMemory.Business
{
    public class MainMemory
    {
        private List<byte> memoryDump;
        private int memoryLocationsNum;
        private int stackLocationsNum;
        private int interruptTableSegment;
        private int interruptHandlersSegment;
        private int dataSegment;
        private int codeSegment;
        private int stackSegment;

        private int interuptsNum = 16;
        private int maxInterruptCodeSize = 100; // size in bytes
        private int maxDataSegmentSize = 10000; // idem
        private int maxCodeSegmentSize = 10000; // idem
        private int defaultStackSize = 100; //idem
        private int stackPointer;
        private int freeMemorySpace;

        private static MainMemory? mainMemoryInstance;
        private static readonly Lock _lock = new();
        private MainMemory()
        {

            // Basic memory layout:
            //     Free memory
            //     Stack area
            //     Code segment
            //     Data segment
            //     Interrupt handlers
            //     Interupt vector table

            this.memoryLocationsNum = 1 << 16; // 16 bit address bus
            this.memoryDump = new List<byte>(this.memoryLocationsNum);
            this.interruptTableSegment = 0;
            this.interruptHandlersSegment = this.interruptTableSegment + this.interuptsNum;
            this.dataSegment = this.interruptHandlersSegment + this.interuptsNum * this.maxInterruptCodeSize;
            this.codeSegment = this.dataSegment + this.maxDataSegmentSize;
            this.stackSegment = this.codeSegment + this.maxCodeSegmentSize;
            this.stackLocationsNum = this.defaultStackSize;
            this.stackPointer = this.stackSegment - 2;
            // the stack pointer always
            // points outside of the stack area
            // so to be correctly alligned for the first push in stack
            this.freeMemorySpace = this.memoryLocationsNum - this.stackLocationsNum;
        }

        public static MainMemory GetMainMemoryInstance()
        {

            lock (_lock) // ensure that only one thread can access this portion of code
            {
                if (mainMemoryInstance == null)
                {

                    mainMemoryInstance = new MainMemory();
                }
            }
            return mainMemoryInstance;
        }

        public void SetInternalStackSize(int stackSize)
        {

            if (stackSize + this.freeMemorySpace > this.memoryLocationsNum)
                throw new InvalidOperationException("Stack size too large! Please try another value.");

            this.stackLocationsNum = stackSize;
            this.freeMemorySpace = this.memoryLocationsNum - this.stackLocationsNum;
        }

        public void SetInternalData(int address, byte content)
        {
            if( address > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(address), "Address is out of range. Please try another value");

            this.memoryDump[address] = content;
        }

        public void SetInternalMachineCode(List<byte> machineCode)
        {
            if(machineCode.Count > this.memoryDump.Count)
                throw new InvalidOperationException("Machine code size exceeds memory capacity. Please try another program.");

            for (int i = 0; i < memoryDump.Count; i++)
                this.memoryDump[i] = machineCode[i];
        }

        public List<byte> GetInternalMemoryDump()
        {
            return this.memoryDump;
        }

        public void CleanInternalMemory()
        {
            for (int i = 0; i < this.memoryDump.Count; i++)
                this.memoryDump[i] = 0;
        }
    }
}
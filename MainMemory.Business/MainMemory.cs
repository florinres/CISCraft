
using System.Net;

namespace MainMemory.Business
{
    public class MainMemory
    {
        private byte[] memoryDump;
        private int memoryLocationsNum;

        private ushort interruptTableSegment;
        private ushort dataSegment;
        private ushort codeSegment;
        private ushort interruptRoutinesSegment; //ISRs
        private ushort freeMemorySegemnt;
        private ushort stackSegment;

        private ushort stackBottom;

        private int interuptsNum;
        private int maxInterruptSize;
        private ushort retiOpCode; // OpCode for Return Interrupt instruction

        private static MainMemory? mainMemoryInstance;
        private static readonly Lock _lock = new();
        private MainMemory()
        {

            // Basic memory layout:
            //     Stack segment
            //     Free memory
            //     Interrupt Service Routines
            //     Code segment
            //     Data segment
            //     Interupt vector table

            //Initialize memory parameters
            this.memoryLocationsNum = 1 << 16; // 16 bit address bus
            this.memoryDump = new byte[this.memoryLocationsNum];
            this.retiOpCode = 0xe00f;
            this.interuptsNum = 16;
            this.maxInterruptSize = 0x1db9;

            // Initialize memory segments

            this.interruptTableSegment = 0x0000;
            this.dataSegment = 0x001f;
            this.codeSegment = 0x1234;
            this.interruptRoutinesSegment = 0x2fed;
            // recomending the ISRs be short, we shall
            // take half of this segment and give to
            // code segment
            this.freeMemorySegemnt = ((ushort)((this.memoryLocationsNum - 1 - this.codeSegment) / 4));
            this.stackSegment = ((ushort)((this.memoryLocationsNum - 1)));

            this.stackBottom = ((ushort)((this.memoryLocationsNum - 1)));
            // similar to Intel's 8086, the stack
            // shall go downwards

            //Initialize ISRs

            for (int i = 0; i < this.interuptsNum; i++)
                this.InitializeISR(i);

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

            if (this.stackSegment - stackSize < this.freeMemorySegemnt)
                throw new InvalidOperationException("Stack size too large! Please try another value.");

            this.stackBottom = ((ushort)((this.memoryLocationsNum - stackSize)));
        }

        public void SetInternalData(int address, byte content)
        {
            if( address > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(address), "Address is out of range. Please try another value.");

            this.memoryDump[address] = content;
        }

        public void SetInternalMachineCode(byte[] machineCode)
        {
            if(machineCode.Length > this.interruptRoutinesSegment - this.codeSegment)
                throw new InvalidOperationException("Machine code size exceeds memory capacity. Please try another program.");

            for (int i = 0; i < machineCode.Length; i++)
                this.memoryDump[this.codeSegment + i] = machineCode[i];
        }

        public byte[] GetInternalMemoryDump()
        {
            return this.memoryDump;
        }

        public void CleanInternalMemory()
        {
            for (int i = 0; i < this.memoryDump.Length; i++)
                this.memoryDump[i] = 0;
        }

        public byte GetInternalLocationData(int memoryAddress)
        {
            if (memoryAddress > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(memoryAddress), "Address is out of range. Please try another value.");

            return this.memoryDump[memoryAddress];
        }

        public int GetMemorySize()
        {
            return this.memoryLocationsNum;
        }

        public void SetInternalISR(int interruptNumber, byte[] interruptRoutine)
        {
            if(interruptNumber > this.interuptsNum)
                throw new ArgumentOutOfRangeException(nameof(interruptNumber), "Interrupt number exceeds the total number of interrupts. Please try another value.");

            ushort isrAddress = (ushort)((this.memoryDump[interruptNumber * 2 + 1] << 8) | this.memoryDump[interruptNumber * 2]); //little endian addressing

            if(isrAddress + interruptRoutine.Length > this.maxInterruptSize)
                throw new ArgumentOutOfRangeException(nameof(interruptNumber), "Interrupt exceeds the maximum allocated space. Please try adding a smaller routine.");

            for (int i = 0; i < interruptRoutine.Length; i++)
                this.memoryDump[isrAddress + i] = interruptRoutine[i];

        }

        public void ClearInternlISR(int interruptNumber)
        {
            if(interruptNumber > this.interuptsNum)
                throw new ArgumentOutOfRangeException(nameof(interruptNumber), "Interrupt number exceeds the total number of interrupts. Please try another value.");

            ushort isrAddress = (ushort)((this.memoryDump[interruptNumber * 2 + 1] << 8) | this.memoryDump[interruptNumber * 2]); //little endian addressing

            for (int i = isrAddress; i < this.maxInterruptSize - 1; i += 2)
            {
                this.memoryDump[i] = (byte)(this.retiOpCode & 0xFF);
                this.memoryDump[i + 1] = (byte)((this.retiOpCode >> 8) & 0xFF);
            }
        }
        private void InitializeISR(int interruptNumber)
        {
            ushort isrAddress = (ushort)((this.memoryDump[interruptNumber * 2 + 1] << 8) | this.memoryDump[interruptNumber * 2]);

            for (int i = isrAddress; i < this.maxInterruptSize - 1; i+=2)
            {
                this.memoryDump[i] = (byte)(this.retiOpCode & 0xff);
                this.memoryDump[i + 1] = (byte)(this.retiOpCode >> 8);
            }

        }
    }
}
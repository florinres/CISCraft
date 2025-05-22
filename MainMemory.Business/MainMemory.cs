using System.Net;
using MainMemory.Business.Models;

namespace MainMemory.Business
{
    public class MainMemory: IMainMemory
    {
        private MomeryContentWrapper _memoryContent;
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
        public MainMemory(MomeryContentWrapper memoryWrapperContent)
        {

            // Basic memory layout:
            //     Stack segment
            //     Free memory
            //     Interrupt Service Routines
            //     Code segment
            //     Data segment
            //     Interupt vector table

            //Initialize memory parameters
            this._memoryContent = memoryWrapperContent;
            this.retiOpCode = 0xe00f;
            this.interuptsNum = 16;
            this.maxInterruptSize = 0x1db9;

            // Initialize memory segments

            this.interruptTableSegment = 0x0000;
            this.dataSegment = 0x001f;
            //this.codeSegment = 0x1234;
            this.codeSegment = 0;
            this.interruptRoutinesSegment = 0x2fed;
            // recomending that the ISRs be short, we shall
            // take half of this segment and give to
            // code segment
            this.memoryLocationsNum = 1 << 16;
            this.freeMemorySegemnt = ((ushort)((this.memoryLocationsNum - 1 - this.codeSegment) / 4));
            this.stackSegment = ((ushort)((this.memoryLocationsNum - 1)));

            this.stackBottom = ((ushort)((this.memoryLocationsNum - 1)));
            // similar to Intel's 8086, the stack
            // shall go downwards

            //Initialize ISRs

            for (int i = 0; i < this.interuptsNum; i++)
                this.InitializeISR(i);

        }


        public void SetStackSize(int stackSize)
        {

            if (this.stackSegment - stackSize < this.freeMemorySegemnt)
                throw new InvalidOperationException("Stack size too large! Please try another value.");

            this.stackBottom = ((ushort)((this.memoryLocationsNum - stackSize)));
        }

        public void SetByteLocation(int address, byte content)
        {
            if( address > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(address), "Address is out of range. Please try another value.");

            this._memoryContent[address] = content;
        }

        public void SetWordLocation(int address, short content)
        {
            if (address > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(address), "Address is out of range. Please try another value.");

            this._memoryContent[address+1] = (byte)(content>>8);
            this._memoryContent[address] = (byte)(content << 8 >>8);
        }

        public void LoadMachineCode(byte[] machineCode)
        {
            if(machineCode.Length > this.interruptRoutinesSegment - this.codeSegment)
                throw new InvalidOperationException("Machine code size exceeds memory capacity. Please try another program.");

            _memoryContent.NotifyChange = false;
            for (int i = 0; i < machineCode.Length; i++)
                this._memoryContent[this.codeSegment + i] = machineCode[i];
            _memoryContent.NotifyChange = true;
        }

        public byte[] GetMemoryDump()
        {
            return this._memoryContent.MemoryContent;
        }

        public void ClearMemory()
        {
            _memoryContent.NotifyChange = false;
            for (int i = 0; i < this._memoryContent.Length; i++)
                this._memoryContent[i] = 0;
            _memoryContent.NotifyChange = true;
        }

        public byte FetchByte(int memoryAddress)
        {
            if (memoryAddress > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(memoryAddress), "Address is out of range. Please try another value.");

            return this._memoryContent[memoryAddress];
        }

        public short FetchWord(int memoryAddress)
        {
            if (memoryAddress > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(memoryAddress), "Address is out of range. Please try another value.");

            return (short)((this._memoryContent[memoryAddress + 1] << 8) | this._memoryContent[memoryAddress]);
        }

        public int GetMemorySize()
        {
            return this.memoryLocationsNum;
        }

        public void SetISR(int interruptNumber, byte[] interruptRoutine)
        {
            if(interruptNumber > this.interuptsNum)
                throw new ArgumentOutOfRangeException(nameof(interruptNumber), "Interrupt number exceeds the total number of interrupts. Please try another value.");

            ushort isrAddress = (ushort)((this._memoryContent[interruptNumber * 2 + 1] << 8) | this._memoryContent[interruptNumber * 2]); //little endian addressing

            if(isrAddress + interruptRoutine.Length > this.maxInterruptSize)
                throw new ArgumentOutOfRangeException(nameof(interruptNumber), "Interrupt exceeds the maximum allocated space. Please try adding a smaller routine.");

            for (int i = 0; i < interruptRoutine.Length; i++)
                this._memoryContent[isrAddress + i] = interruptRoutine[i];

        }

        public void ClearISR(int interruptNumber)
        {
            if(interruptNumber > this.interuptsNum)
                throw new ArgumentOutOfRangeException(nameof(interruptNumber), "Interrupt number exceeds the total number of interrupts. Please try another value.");

            ushort isrAddress = (ushort)((this._memoryContent[interruptNumber * 2 + 1] << 8) | this._memoryContent[interruptNumber * 2]); //little endian addressing
            _memoryContent.NotifyChange = false;
            for (int i = isrAddress; i < this.maxInterruptSize - 1; i += 2)
            {
                this._memoryContent[i] = (byte)(this.retiOpCode & 0xFF);
                this._memoryContent[i + 1] = (byte)((this.retiOpCode >> 8) & 0xFF);
            }
            _memoryContent.NotifyChange = true;
        }
        private void InitializeISR(int interruptNumber)
        {
            ushort isrAddress = (ushort)((this._memoryContent[interruptNumber * 2 + 1] << 8) | this._memoryContent[interruptNumber * 2]);

            _memoryContent.NotifyChange = false;
            for (int i = isrAddress; i < this.maxInterruptSize - 1; i+=2)
            {
                this._memoryContent[i] = (byte)(this.retiOpCode & 0xff);
                this._memoryContent[i + 1] = (byte)(this.retiOpCode >> 8);
            }
            _memoryContent.NotifyChange = true;

        }
    }
}
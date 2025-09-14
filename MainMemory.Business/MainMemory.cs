using System.Net;
using System.Text.Json;
using MainMemory.Business.Models;

namespace MainMemory.Business
{
    public class MainMemory: IMainMemory
    {
        private MemoryContentWrapper _memoryContent;
        public int memoryLocationsNum { get; private set; }

        private ushort _ivtSegment; // Interrupt Vector Table
        private ushort _userCodeSegment;
        private ushort _interruptHandlersSegment;
        private ushort _stackSegment;
        private int _interuptsNum;
        private ushort _retiOpCode; // OpCode for Return Interrupt instruction
        private string _ivtConfigFile;

        /// <summary>
        /// Class responsible for modelling the behaviour
        /// of RAM or main memory block.
        /// </summary>
        /// <param name="memoryWrapperContent"></param>
        public MainMemory(MemoryContentWrapper memoryWrapperContent)
        {

            // Basic memory layout:
            //     Stack
            //     Interrupt Handlers
            //     User Code
            //     Interupt Vector Table

            //Initialize memory parameters
            this._memoryContent = memoryWrapperContent;
            this._retiOpCode = 0xe00f;
            this.memoryLocationsNum = 1 << 16;
            this._interuptsNum = 4;
            this._ivtConfigFile = Path.GetFullPath(AppContext.BaseDirectory + "/../../../../Configs/IVT.json");
            ushort defaultAddressOffset = (ushort)((this.memoryLocationsNum - 0x000F) / 3);

            // Initialize memory segments with default values

            this._ivtSegment = 0x0000; // 4 exceptions and 4 possible IRQs
                                       // each having 2 byte addresses
            this._userCodeSegment = 0x0010;
            this._interruptHandlersSegment = (ushort)(this._userCodeSegment + defaultAddressOffset);
            this._stackSegment = (ushort)(this._interruptHandlersSegment + defaultAddressOffset); // similar to Intel's 8086, the stack
                                                                                                  // shall go downwards
            InitializeDefaultInterrupts();

        }
        /// <summary>
        /// Sets the given 8 bits into the given adddress location.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="content"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetByteLocation(int address, byte content)
        {
            if( address > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(address), "Address is out of range. Please try another value.");

            this._memoryContent[address] = content;
        }
        /// <summary>
        /// Sets the given 16 bits into the given adddress location.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="content"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetWordLocation(int address, short content)
        {
            if (address > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(address), "Address is out of range. Please try another value.");

            this._memoryContent[address+1] = (byte)(content>>8);
            this._memoryContent[address] = (byte)(content << 8 >>8);
        }
        /// <summary>
        /// Loads user machine code into main memory.
        /// </summary>
        /// <param name="machineCode"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void LoadMachineCode(byte[] machineCode)
        {
            if(machineCode.Length > this._interruptHandlersSegment - this._userCodeSegment)
                throw new InvalidOperationException("User machine code size exceeds memory capacity. Please try another program.");

            _memoryContent.NotifyChange = false;
            for (int i = 0; i < machineCode.Length; i++)
                this._memoryContent[this._userCodeSegment + i] = machineCode[i];
            _memoryContent.NotifyChange = true;
        }

        /// <summary>
        /// Returns main memory dump in byte form.
        /// </summary>
        /// <returns></returns>
        public byte[] GetMemoryDump()
        {
            return this._memoryContent.MemoryContent;
        }
        /// <summary>
        /// Clears all of the memory contents.
        /// </summary>
        public void ClearMemory()
        {
            _memoryContent.NotifyChange = false;
            for (int i = 0; i < this._memoryContent.Length; i++)
                this._memoryContent[i] = 0;
            _memoryContent.NotifyChange = true;
        }
        /// <summary>
        /// Fettches a half word of 8 bits from a given address.
        /// </summary>
        /// <param name="memoryAddress"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte FetchByte(int memoryAddress)
        {
            if (memoryAddress > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(memoryAddress), "Address is out of range. Please try another value.");

            return this._memoryContent[memoryAddress];
        }
        /// <summary>
        /// Fetches a 16 bit word from a given address.
        /// </summary>
        /// <param name="memoryAddress"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public short FetchWord(int memoryAddress)
        {
            if (memoryAddress > this.memoryLocationsNum - 1)
                throw new ArgumentOutOfRangeException(nameof(memoryAddress), "Address is out of range. Please try another value.");

            return (short)((this._memoryContent[memoryAddress + 1] << 8) | this._memoryContent[memoryAddress]);
        }
        /// <summary>
        /// Returns the total size of program memory.
        /// </summary>
        /// <returns></returns>
        public int GetMemorySize()
        {
            return this.memoryLocationsNum;
        }
        /// <summary>
        /// Sets the interrupt handler based on the given interrupt number.
        /// </summary>
        /// <param name="interruptNumber"></param>
        /// <param name="interruptRoutine"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetISR(int interruptNumber, byte[] interruptRoutine)
        {
            if(interruptNumber > this._interuptsNum)
                throw new ArgumentOutOfRangeException(nameof(interruptNumber), "Interrupt number exceeds the total number of interrupts. Please try another value.");

            ushort isrAddress = (ushort)((this._memoryContent[interruptNumber * 2 + 1] << 8) | this._memoryContent[interruptNumber * 2]); //little endian addressing

            for (int i = 0; i < interruptRoutine.Length; i++)
                this._memoryContent[isrAddress + i] = interruptRoutine[i];

        }

        /// <summary>
        /// Initializes the addresses for IVT
        /// and content of interrupt handlers
        /// using default values.
        /// </summary>
        private void InitializeDefaultInterrupts()
        {
            string configFile = File.ReadAllText(this._ivtConfigFile);

            var table = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(configFile);
            foreach (var irqEntry in table)
            {
                var irqData = irqEntry.Value;

                int memoryStartAddress = Convert.ToInt32(irqData["memoryStartAddress"],16);
                int handlerStartAddress = Convert.ToInt32(irqData["handlerStartAddress"],16);
                string handlerCode = irqData["handlerCode"];

                //little endian
                this._memoryContent[memoryStartAddress] = (byte)((handlerStartAddress << 8) >> 8);
                this._memoryContent[memoryStartAddress + 1] = (byte)(handlerStartAddress >> 8);
            }
        }
    }
}
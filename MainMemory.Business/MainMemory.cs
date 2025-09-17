using System.Net;
using System.Text.Json;
using MainMemory.Business.Models;

namespace MainMemory.Business
{
    public class MainMemory : IMainMemory
    {
        private MemoryContentWrapper _memoryContent;

        private static ushort s_memorySize = 1 << 16 - 1; // The size of the whole RAM (i.e 64Kb)
        private ushort _ivtSegment; // Interrupt Vector Table
        private ushort _userCodeSegment;
        private ushort _interruptHandlersSegment;
        private ushort _stackSegment;
        private int _interuptsNum;
        private ushort _retiOpCode; // OpCode for Return Interrupt instruction
        private string _ivtConfigFile;
        public List<ISR>? Isrs;
        private static ushort s_segmentSize = (ushort)((s_memorySize - 0x000F) / 3); // The size of a segment (i.e 64KB - (16 bytes - IVT size ) / 3 = 21.84 Kb)
                                                                                     // The 3 segments: 
                                                                                     // - User Code
                                                                                     // - ISR segment (all 8 ISRs will be there, so 21.82 / 8 = 2.73 Kb per ISR, that is 459 lines of code, worst case scenario)
                                                                                     // - Stack Segment
        private static ushort s_isrSectionSize = (ushort)(s_segmentSize / 8);

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
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
            this._interuptsNum = 4;
            this._ivtConfigFile = Path.GetFullPath(AppContext.BaseDirectory + "/../../../../Configs/IVT.json");

            // Initialize memory segments with default values
            this._ivtSegment = 0x0000; // 4 exceptions and 4 possible IRQs
                                       // each having 2 byte addresses
            this._userCodeSegment = 0x0010;
            this._interruptHandlersSegment = (ushort)(this._userCodeSegment + s_segmentSize - 1);
            this._stackSegment = (ushort)(this._interruptHandlersSegment + s_segmentSize + 1); // similar to Intel's 8086, the stack
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
            if (address > s_memorySize - 1)
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
            if (address > s_memorySize)
                throw new ArgumentOutOfRangeException(nameof(address), "Address is out of range. Please try another value.");

            this._memoryContent[address + 1] = (byte)(content >> 8);
            this._memoryContent[address] = (byte)(content << 8 >> 8);
        }
        /// <summary>
        /// Loads binary code at a given section
        /// </summary>
        /// <param name="machineCode"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void LoadAtOffset(byte[] machineCode, ushort sectionOffset)
        {
            if (sectionOffset > this._interruptHandlersSegment && machineCode.Length > s_isrSectionSize)
                throw new InvalidOperationException("Machine code exceeds the isr section size");
            if (machineCode.Length > s_segmentSize)
                throw new InvalidOperationException("Machine code exceeds the segment size");

            _memoryContent.NotifyChange = false;
            for (int i = 0; i < machineCode.Length; i++)
                this._memoryContent[sectionOffset + i] = machineCode[i];
            _memoryContent.NotifyChange = true;
        }

        /// <summary>
        /// Loads user machine code into main memory.
        /// </summary>
        /// <param name="machineCode"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void LoadMachineCode(byte[] machineCode)
        {
            if (machineCode.Length > this._interruptHandlersSegment - this._userCodeSegment)
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
            if (memoryAddress > s_memorySize)
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
            if (memoryAddress > s_memorySize)
                throw new ArgumentOutOfRangeException(nameof(memoryAddress), "Address is out of range. Please try another value.");

            return (short)((this._memoryContent[memoryAddress + 1] << 8) | this._memoryContent[memoryAddress]);
        }
        /// <summary>
        /// Returns the total size of program memory.
        /// </summary>
        /// <returns></returns>
        public int GetMemorySize()
        {
            return s_memorySize;
        }
        /// <summary>
        /// Sets the interrupt handler for HW interrupts or exceptions based on given address.
        /// </summary>
        /// <param name="handlerAddress"></param>
        /// <param name="interruptRoutine"></param>
        public void SetISR(int handlerAddress, byte[] interruptRoutine)
        {
            for (int i = 0; i < interruptRoutine.Length; i++)
                this._memoryContent[handlerAddress + i] = interruptRoutine[i];
        }

        /// <summary>
        /// Initializes the addresses for IVT
        /// and content of interrupt handlers
        /// using default values.
        /// </summary>
        private void InitializeDefaultInterrupts()
        {
            this.Isrs = ReadIVTJson();
            foreach (var isr in Isrs)
            {
                ushort memoryStartAddress = isr.IVTAddress;
                ushort handlerStartAddress = isr.ISRAddress;

                //little endian
                this._memoryContent[memoryStartAddress] = (byte)((handlerStartAddress << 8) >> 8);
                this._memoryContent[memoryStartAddress + 1] = (byte)(handlerStartAddress >> 8);
            }
        }

        private List<ISR> ReadIVTJson()
        {
            string currentFolder = Path.GetFullPath(AppContext.BaseDirectory + "../../../../");
            string jsonPath = Path.Combine(currentFolder + "Configs", "IVT.json");
            if (!File.Exists(jsonPath))
                return new List<ISR>();
            string json = File.ReadAllText(jsonPath);

            return JsonSerializer.Deserialize<List<ISR>>(json, JsonOpts) ?? new();
        }
    }
}
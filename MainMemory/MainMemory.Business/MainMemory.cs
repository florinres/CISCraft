namespace MainMemory.Business
{
    public class MainMemory
    {
        private List<byte> memoryDump;
        private int memoryLocationsNum;
        private int stackLocationsNum;
        private int interruptTableSegment;
        private int interruptHandlersSegment;
        private int dataSegemnt;
        private int codeSegment;
        private int stackSegment;

        private int interuptsNum = 16;
        private int stackPointer;

        public MainMemory()
        {
            this.memoryLocationsNum = 1 << 16; // 16 bit address bus
            this.memoryDump = new List<byte>(this.memoryLocationsNum);
            this.interruptTableSegment = 0;
            this.interruptHandlersSegment = this.interuptsNum;
            this.dataSegemnt = 1600;
            this.codeSegment = 330068;
            this.stackSegment = 34068;
            this.stackPointer = this.stackSegment - 2;
            // the stack pointer always
            // points outside of the stack area
            // so to be correctly alligned for the first push in stack
        }
    }
}

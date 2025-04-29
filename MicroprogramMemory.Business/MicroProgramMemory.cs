namespace MicroprogramMemory.Business
{
    public class MicroProgramMemory
    {
        Dictionary<string,ulong> microMemoryDump;

        private static MicroProgramMemory? microMemoryInstance;
        private static readonly Lock _lock = new();

        private MicroProgramMemory()
        {
            this.microMemoryDump = new Dictionary<string, ulong>();
        }

        public static MicroProgramMemory GetMicroMemoryInstance()
        {
            lock (_lock) // ensure that only one thread can access this portion of code
            {
                if (microMemoryInstance == null)
                {

                    microMemoryInstance = new MicroProgramMemory();
                }
            }
            return microMemoryInstance;
        }
    }
}

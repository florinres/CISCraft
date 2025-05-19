namespace MainMemory.Business
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //MainMemory mainMemory = MainMemory.GetMainMemoryInstance();
            long memoryStart = System.GC.GetTotalMemory(true);
            byte[] byteArray = new byte[1<<16];
            //List<byte> byteList = new List<byte>(1 << 16);

            long memoryEnd = System.GC.GetTotalMemory(true);
            Console.WriteLine("Roughly {0} bytes of memory are used.", memoryEnd - memoryStart - 8); //Check how much memory was used by the variable

        }

    }

}
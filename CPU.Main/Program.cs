using System.Security.Cryptography;
using Cpu = CPU.Business.CPU;
using Ram = MainMemory.Business.MainMemory;
using ASM = Assembler.Business.Assembler;
namespace CPU.Main
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ASM assembler = new ASM();
            Ram ram = new Ram();
            Cpu cpu = new Cpu(ram);
            
            string filePath = "main.s";
            string file;
            byte[] objectCode = new byte[200];
            int len = 0;
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    file = reader.ReadToEnd();
                    objectCode = assembler.Assemble(file, out len);
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: File not found - {ex.Message}");
            }

            ram.LoadMachineCode(objectCode);

            string jsonString = File.ReadAllText("C:\\Users\\rudy\\Desktop\\CISCraft\\Configs\\MPM.json");
            cpu.LoadJsonMpm(jsonString);

            for (int i = 0; i < 100; i++)
            {
                int a, b;
                (a, b) = cpu.StepMicrocode();
                Console.WriteLine("("+a+", "+b+")");
            }
        }
    }
}
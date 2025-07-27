using System.Security.Cryptography;
using Cpu = CPU.Business.CPU;
using Ram = MainMemory.Business.MainMemory;
using MemWrapper = MainMemory.Business.Models.MomeryContentWrapper;
using ASM = Assembler.Business.Assembler;
using CPU.Business.Models;
namespace CPU.Main
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ASM assembler = new ASM();
            MemWrapper memWrapper = new MemWrapper();
            Ram ram = new Ram(memWrapper);
            RegisterWrapper list = new RegisterWrapper(20);
            Cpu cpu = new Cpu(ram,list);

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
//todo: important
            ram.LoadMachineCode(objectCode);

            string jsonString = File.ReadAllText("C:\\Users\\rudy\\Desktop\\CISCraft\\Configs\\MPM.json");
            //todo: mpm
            cpu.LoadJsonMpm(jsonString);
            int a, b;
            int i = 0;

            for(int j=0;j<2; j++)
            {
                (a, b) = cpu.StepMicrocommand();
                Console.WriteLine(cpu.GetCurrentLabel(a));
                Console.WriteLine(i + ": (" + a + ", " + b + ")");
                i++;
                (a, b) = cpu.StepMicrocommand();
                Console.WriteLine(cpu.GetCurrentLabel(a));
                Console.WriteLine(i + ": (" + a + ", " + b + ")");
                i++;
                while ((a != 0) || (b != 0))
                {
                    (a, b) = cpu.StepMicrocommand();
                    Console.WriteLine(cpu.GetCurrentLabel(a));
                    Console.WriteLine(i + ": (" + a + ", " + b + ")");
                    i++;
                }
                Console.WriteLine();
            }
        }
    }
}
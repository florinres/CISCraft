using System.Security.Cryptography;
using Cpu = CPU.Business.CPU;
using Ram = MainMemory.Business.MainMemory;
using ASM = Assembler.Business.Assembler;
using CPU.Business.Models;
namespace CPU.Main
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ASM assembler = new ASM();
            Ram ram = new Ram();
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

            for (int i = 0; i < 120; i++)
            {
                int a, b;
                (a, b) = cpu.StepMicrocommand();
                Console.WriteLine(i+": ("+a+", "+b+")");
            }
        }
    }
}
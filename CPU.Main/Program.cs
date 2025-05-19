using System.Security.Cryptography;
using Cpu = CPU.Business.CPU;
namespace CPU.Main
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Cpu cpu = new Cpu();
            string jsonString = File.ReadAllText("C:\\Users\\rudy\\Desktop\\CISCraft\\Configs\\MPM.json");
            cpu.LoadJsonMpm(jsonString);
            cpu.StepMicrocode();
        }
    }
}
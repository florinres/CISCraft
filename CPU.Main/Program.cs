using Cpu = CPU.Business.CPU;
namespace CPU.Main
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Cpu cpu = new Cpu();
            cpu.StepMicrocode();
        }
    }
}
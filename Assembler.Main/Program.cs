using System;
using Irony.Parsing;

namespace Assembler.Business
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "main.s";
            string file;
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    file = reader.ReadToEnd();
                    Assembler assembler = new Assembler();
                    assembler.Assemble(file);
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: File not found - {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }
    }
}
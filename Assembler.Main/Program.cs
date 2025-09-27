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
            byte[] objectCode = new byte[200];
            int len = 0;
            List<AssemblyError> errors;
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    file = reader.ReadToEnd();
                    Assembler assembler = new Assembler();
                    objectCode = assembler.Assemble(file, out len, out errors);
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: File not found - {ex.Message}");
            }
            if(len != 0)
            {
                using (FileStream fs = new FileStream("main.obj", FileMode.Create))
                {
                    fs.Write(objectCode, 0, len);
                }
            }
        }
    }
}
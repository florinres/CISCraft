using System;
using System.Diagnostics;
using System.IO;

namespace Assembler.Business
{
    public enum WriteMode
    {
        Append,
        Overwrite
    }

    public class LogWriter
    {
        public LogWriter() { }
        public void LogInfo(string message, string filePath, WriteMode mode = WriteMode.Append)
        {
            try
            {
                switch (mode)
                {
                    case WriteMode.Append:
                        File.AppendAllText(filePath, message + Environment.NewLine);
                        break;
                    case WriteMode.Overwrite:
                        File.WriteAllText(filePath, message + Environment.NewLine);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file LogWriter.cs: {ex.Message}");
                Debug.WriteLine($"Error writing to file LogWriter.cs: {ex.Message}");
            }
        }
    }
}

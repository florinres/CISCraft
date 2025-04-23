using System.Text.Json;

namespace Translator
{
    public class Translator
    {
        private List<Directive> directives;
        public Translator()
        {
            string microcode = "microcode.json";
            string json = File.ReadAllText(microcode);
            directives = JsonSerializer.Deserialize<List<Directive>>(json);
        }

        //string[] mov = { "0000", "01", "0000", "01", "0001" }; //mov R0, R0
        //string[] clr = { "1010", "01", "0001", "01", "0000" }; //clr R0
        //string[] beq = { "1100", "01", "0001", "01", "0010" }; //beq ..
        //string[] clc = { "1110", "00", "0011", "11", "0111" }; //clc
        void GenerateMicrocode(string[] IR)
        {
            string[] processorCommands;
            switch (Instruction.DetermineClass(IR))
            {
                case Instruction.InstructionClass.ClassA:
                    break;
                case Instruction.InstructionClass.ClassB:
                    break;
                case Instruction.InstructionClass.ClassC:
                    break;
                case Instruction.InstructionClass.ClassD:
                    break;
                case null:
                    break;
            }
        }

        [Obsolete("Instruction will be read from IR register")]
        public async void TranslateInputFile(string path)
        {
            string[] codeLines = await File.ReadAllLinesAsync(path);
            Queue<int> otherProcsIndexes = new Queue<int>();
            int procLineNumber = -1;
            int endpStartLineNumber = -1;
            bool foundProcLine = false;
            bool foundEndpLine = false;
            char[] separators = { ' ', ',' };
            string[] tokens;
            List<string> processorCommands = new List<string>();
            //Standard search for main proc
            for (int i = 0; i < codeLines.Length; i++)
            {
                if (codeLines[i].Equals("proc start"))
                {
                    procLineNumber = i;
                    foundProcLine = true;
                }
                if (codeLines[i].Equals("endp start"))
                {
                    endpStartLineNumber = i;
                    foundEndpLine = true;
                }
            }

            if (!foundEndpLine || !foundProcLine)
            {
                throw new TranslatorException("Main procedure not found!");
            }

            //first produce the mnemonics for the example program

            for (int i = procLineNumber; i < endpStartLineNumber; i++)
            {
                //if commentary
                if (codeLines[i].StartsWith(";"))
                {
                    continue;
                }
                tokens = codeLines[i].Split(separators);
                for (int j = 0; j < tokens.Length; j++)
                {
                    string instructionToken = tokens[j];
                    foreach (var directive in directives)
                    {
                        if (directive.name == instructionToken)
                        {
                            processorCommands.AddRange(directive.commands);
                            break;
                        }
                    }
                }
            }

            Console.WriteLine("Main commands");
            foreach (var i in processorCommands)
            {
                Console.WriteLine(i);
            }

            bool isInMainProc = false;
            //Find other procs
            for (int i = 0; i < codeLines.Length; i++)
            {
                if (codeLines[i].Equals("endp start"))
                {
                    isInMainProc = false;
                    continue;
                }
                if (isInMainProc)
                {
                    continue;
                }
                if (codeLines[i].Equals("proc start"))
                {
                    isInMainProc = true;
                    continue;
                }
                if (codeLines[i].StartsWith("proc"))
                {
                    otherProcsIndexes.Enqueue(i);
                }
                if (codeLines[i].StartsWith("endp"))
                {
                    otherProcsIndexes.Enqueue(i);
                }
            }

            //it means that one procedure is not ended correctly
            if (otherProcsIndexes.Count / 2 != 0)
            {
                throw new TranslatorException("Procedures have no end point!");
            }

            Console.WriteLine("Other procedures commands");

            while (otherProcsIndexes.Count > 0)
            {
                int startIndex = otherProcsIndexes.Dequeue();
                int endIndex = otherProcsIndexes.Dequeue();

                List<string> procedureCommands = new List<string>();

                for (int i = startIndex + 1; i < endIndex; i++)
                {
                    // Skip comments
                    if (codeLines[i].StartsWith(";"))
                    {
                        continue;
                    }

                    tokens = codeLines[i].Split(separators);
                    foreach (string token in tokens)
                    {
                        foreach (var directive in directives)
                        {
                            if (directive.name == token)
                            {
                                procedureCommands.AddRange(directive.commands);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}

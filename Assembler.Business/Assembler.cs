using Irony.Parsing;

namespace Assembler.Business
{
    public class Assembler
    {
        readonly Encoder Encoder;
        readonly CiscGrammar Grammar;
        readonly Parser Parser;
        LogWriter _logWriter;
        private string _logsDir;
        private string _parserLog;
        public static ushort ProgramStartingAddress
        {
            get
            {
                return Encoder.s_programStartingAddress;
            }
            set
            {
                Encoder.s_programStartingAddress = value;
            }
        }

        public Assembler()
        {
            Encoder = new Encoder();
            Grammar = new CiscGrammar();
            Parser  = new Parser(Grammar);
            _logWriter = new LogWriter();
            _logsDir = Path.GetFullPath(AppContext.BaseDirectory + "../../../../Logs");
            _parserLog = "parser.log";
        }
        public byte[] Assemble(string sourceCode, out int len, out List<AssemblyError> errors)
        {
            var tree = Parser.Parse(sourceCode);

            errors = new List<AssemblyError>();

            if (tree.HasErrors())
            {
                foreach (var err in tree.ParserMessages)
                {
                    if (err.Location.Column > 1)
                    {
                        errors.Add(new AssemblyError
                        {
                            // Add 1 to line number to make it 1-based
                            Line = err.Location.Line + 2,
                            Column = err.Location.Column + 2, // Also adjust column to be 1-based
                            Length = 1000,
                            Message = err.Message
                        });
                    }
                    else
                    {
                        errors.Add(new AssemblyError
                        {
                            // Add 1 to line number to make it 1-based
                            Line = err.Location.Line + 1,
                            Column = err.Location.Column + 1, // Also adjust column to be 1-based
                            Length = 1000,
                            Message = err.Message
                        });
                    }
                }

                len = 0;
                return new byte[0];
            }

            return Encoder.Encode(tree.Root, out len);
        }
        public Dictionary<ushort /* pc */, ushort /* line number */> GetDebugSymbols(ushort pcOffset)
        {
            return Encoder.DebugSymbols.ToDictionary(
                    kvp => (ushort)(kvp.Key + pcOffset),
                    kvp => kvp.Value
                );
        }
    }
}

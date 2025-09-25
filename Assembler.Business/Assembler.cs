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
        public byte[] Assemble(string sourceCode, out int len)
        {
            var tree = Parser.Parse(sourceCode);

            if (tree.HasErrors())
            {
                Dictionary<string,string> parsingErrors = new Dictionary<string,string>();
                foreach (var err in tree.ParserMessages)
                    parsingErrors[err.Message] = err.Location.ToString();
                LogParsingInfo(parsingErrors);
                len = 0;
                return new byte[0];
            }
            else
                LogParsingInfo(new Dictionary<string, string> { { "STATUS:", "Parsing successful." } });

            return Encoder.Encode(tree.Root, out len);
        }
        public Dictionary<ushort /* pc */, ushort /* line number */> GetDebugSymbols(ushort pcOffset)
        {
            return Encoder.DebugSymbols.ToDictionary(
                    kvp => (ushort)(kvp.Key + pcOffset),
                    kvp => kvp.Value
                );
        }

        private void LogParsingInfo(Dictionary<string,string> msg)
        {
            string logPath = _logsDir + "\\" + _parserLog;
            Directory.CreateDirectory(_logsDir);
            File.Create(logPath).Dispose();
            string formatedMsg = DateTime.Now.ToString() + "\n{\n" +
            string.Join(",\n", msg.Select(kvp => $"  \"{kvp.Key}\": \"{kvp.Value}\"")) + 
            "\n}";

            _logWriter.LogInfo(formatedMsg, logPath, WriteMode.Overwrite);
        }
    }
}

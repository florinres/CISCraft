using Irony.Parsing;

namespace Assembler.Business
{
    public class Assembler
    {
        readonly Encoder Encoder;
        readonly CiscGrammar Grammar;
        readonly Parser Parser;
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
        }
        public byte[] Assemble(string sourceCode, out int len)
        {
            var tree = Parser.Parse(sourceCode);

            if (tree.HasErrors())
            {
                foreach (var err in tree.ParserMessages)
                {
                    Console.WriteLine($"Error: {err.Message} at {err.Location}");
                }
                len = 0;
                return new byte[0];
            }
            else
            {
                Console.WriteLine("Parsing successful.");
            }
            return Encoder.Encode(tree.Root, out len);
        }
        public Dictionary<short /* pc */, ushort /* line number */> GetDebugSymbols(ushort pcOffset)
        {
            return Encoder.DebugSymbols.ToDictionary(
                    kvp => (short)(kvp.Key + pcOffset),
                    kvp => kvp.Value
                );
        }
    }
}

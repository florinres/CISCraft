using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Irony.Parsing;

namespace Assembler.Business
{
    public class Assembler
    {
        public ushort programStartingAddress 
        {
            get
            {
                return Encoder.programStartingAddress;
            }
            set
            {
                Encoder.programStartingAddress = value;
            }
        }

        Encoder encoder;
        CiscGrammar grammar;
        Parser parser;
        public Assembler()
        {
            encoder = new Encoder();
            grammar = new CiscGrammar();
            parser  = new Parser(grammar);
        }
        public byte[] Assemble(string sourceCode, out int len)
        {
            var tree = parser.Parse(sourceCode);

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
            return encoder.Encode(tree.Root, out len);
        }
    }
}

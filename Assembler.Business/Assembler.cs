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
        Encoder encoder;
        CiscGrammar grammar;
        Parser parser;
        public Assembler()
        {
            encoder = new Encoder();
            grammar = new CiscGrammar();
            parser  = new Parser(grammar);
        }
        public ushort[] Assemble(string sourceCode)
        {
            var tree = parser.Parse(sourceCode);

            if (tree.HasErrors())
            {
                foreach (var err in tree.ParserMessages)
                {
                    Console.WriteLine($"Error: {err.Message} at {err.Location}");
                }
            }
            else
            {
                Console.WriteLine("Parsing successful.");
            }

            Console.WriteLine("AST Traversal:");
            return encoder.Encode(tree.Root);
        }

    }
}

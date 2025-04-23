<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Irony.Parsing;
=======
﻿using Irony.Parsing;
>>>>>>> origin/feature/integrate_Assembler_with_UI

namespace Assembler.Business
{
    public class Assembler
    {
<<<<<<< HEAD
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
=======
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
>>>>>>> origin/feature/integrate_Assembler_with_UI

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
<<<<<<< HEAD
            return encoder.Encode(tree.Root, out len);
=======
            return Encoder.Encode(tree.Root, out len);
>>>>>>> origin/feature/integrate_Assembler_with_UI
        }
    }
}

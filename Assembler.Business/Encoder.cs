using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Assembler.Business
{
    internal class Encoder
    {
        InstructionParts _instructionParts = default;
        ushort _instrAddress  = 0x0000;
        ushort _symbolAddress = 0x0000;
        ushort _programIndex  = 0x0000;
        byte[] _program       = new byte[300];
        static internal ushort s_programStartingAddress { get; set; } = 0x0000;

        /**
         * @brief  Enumerations for the CASM instructions
         * @note   The CASM instructions are divided into 4 types
         *       B1, B2, B3, B4
         * @details
         *      15 14 13                         0
         *      0  x  x  x x x x x x x x x x x x x
         *      ^ This marks that the _instructionParts is of type B1
         *
         *      15 14 13                         0
         *      1  x  1  x x x x x x x x x x x x x
         *      ^ This marks that the _instructionParts is of type B2
         *
         *      15 14 13                         0
         *      1  1  x  x x x x x x x x x x x x x
         *      ^ ^ This marks that the _instructionParts is of type B3
         *
         *      15 14 13                         0
         *      1  1  1  x x x x x x x x x x x x x
         *      ^ ^ ^ This marks that the _instructionParts is of type B4
         */
        enum OppCodes
        {
            /** B1 INSTRUCTIONS TYPE
             *  MOV, ADD, SUB, CMP, AND, OR, XOR
             *  OPCODE: Operation Code - 4b
             *  AMS: Addressing Mode Source - 2b
             *  SR: Source Register - 4b
             *  AMD: Addressing Mode Destination - 2b
             *  DR: Destination Register - 4b
             *  OPPCODE | AMS | SR | AMD | DR
            */
            MOV = 0x0000, /* 0x0 */
            ADD = 0x1000,
            SUB = 0x2000,
            CMP = 0x3000,
            AND = 0x4000,
            OR = 0x5000,
            XOR = 0x6000,
            /**  B2 INSTRUCTIONS TYPE
             * CLR, NEG, INC, DEC, ASL, ASR, LSR, ROL, ROR, RLC, RRC
             * OPCODE: Operation Code - 10b
             * AMD: Addressing Mode Destination - 2b
             * SD: Destination Register - 4b
             * OPPCODE | AMD | DR
            */
            CLR = 0xA000, /* 0x800 */
            NEG = 0xA100,
            INC = 0xA200,
            DEC = 0xA300,
            ASL = 0xA400,
            ASR = 0xA500,
            LSR = 0xA600,
            ROL = 0xA700,
            ROR = 0xA800,
            RLC = 0xA900,
            RRC = 0xAA00,
            PUSH = 0xAB00,
            POP = 0xAC00,
            /** B3 INSTRUCTIONS TYPE
             * BNE, BEQ, BPL, BMI, BCS, BCC, BVS, BVC, JMP, CALL
             * OPCODE: Operation Code - 8b
             * OFFSET: Offset - 8b
             * OPPCODE | OFFSET
             */
            BNE = 0xC000,
            BEQ = 0xC100,
            BPL = 0xC200,
            BMI = 0xC300,
            BCS = 0xC400,
            BCC = 0xC500,
            BVS = 0xC600,
            BVC = 0xC700,
            JMP = 0xC800,
            CALL = 0xC900,
            /** B4 INSTRUCTIONS TYPE
             * CLC, SEC, HALT, EI, DI, PUSHPC, POPPC, PUSHF,POPF, NOP, RET, RETI
             * WAIT, PUSHPC, POPPC, PUSHF, POPF
             * OPCODE: Operation Code - 16b
             * OPPCODE
             */
            CLC = 0xE000, /* 0xE00 */
            SEC = 0xE208,
            NOP = 0xE400,
            HALT = 0xE600,
            EI = 0xE800,
            DI = 0xEA00,
            PUSHPC = 0xEC00,
            POPPC = 0xEE00,
            PUSHF = 0xF000,
            POPF = 0xF200,
            RET = 0xF400,
            RETI = 0xF600,
            /**
             * ERROR
             */
            ERR = 0xFFFF, /* 0xF00 */
        };
        struct InstructionParts
        {
            public ushort Oppcode;
            public ushort Mas;
            public ushort Mad;
            public ushort Rs;
            public ushort Rd;
            public short  Offset;
            public short  Offset1;
            public short  Offset2;
        }
        struct Instruction
        {
            public ushort Instr;
            public short  Offset1;
            public short  Offset2;
        }
        public readonly Dictionary<string, ushort> SymbolTable = new Dictionary<string, ushort>();
        public readonly Dictionary<short, ushort> DebugSymbols = new Dictionary<short, ushort>();
        readonly Dictionary<string, Dictionary<ushort,string>> _oppcodes = new Dictionary<string, Dictionary<ushort,string>>
        {
            // Numerical value    ,                                     Mnemonic, Type
            { "MOV",    new Dictionary<ushort, string> { {(ushort)OppCodes.MOV ,    "B1" } } },
            { "ADD",    new Dictionary<ushort, string> { {(ushort)OppCodes.ADD ,    "B1" } } },
            { "SUB",    new Dictionary<ushort, string> { {(ushort)OppCodes.SUB ,    "B1" } } },
            { "CMP",    new Dictionary<ushort, string> { {(ushort)OppCodes.CMP ,    "B1" } } },
            { "AND",    new Dictionary<ushort, string> { {(ushort)OppCodes.AND ,    "B1" } } },
            { "OR" ,    new Dictionary<ushort, string> { {(ushort)OppCodes.OR ,     "B1" } } },
            { "XOR",    new Dictionary<ushort, string> { {(ushort)OppCodes.XOR ,    "B1" } } },

            { "CLR",    new Dictionary<ushort, string>{ {(ushort)OppCodes.CLR ,     "B2" } } },
            { "NEG",    new Dictionary<ushort, string> { {(ushort)OppCodes.NEG ,    "B2" } } },
            { "INC",    new Dictionary<ushort, string> { {(ushort)OppCodes.INC ,    "B2" } } },
            { "DEC",    new Dictionary<ushort, string> { {(ushort)OppCodes.DEC ,    "B2" } } },
            { "ASL",    new Dictionary<ushort, string> { {(ushort)OppCodes.ASL ,    "B2" } } },
            { "ASR",    new Dictionary<ushort, string> { {(ushort)OppCodes.ASR ,    "B2" } } },
            { "LSR",    new Dictionary<ushort, string> { {(ushort)OppCodes.LSR ,    "B2" } } },
            { "ROL",    new Dictionary<ushort, string> { {(ushort)OppCodes.ROL ,    "B2" } } },
            { "ROR",    new Dictionary<ushort, string> { {(ushort)OppCodes.ROR ,    "B2" } } },
            { "RLC",    new Dictionary<ushort, string> { {(ushort)OppCodes.RLC ,    "B2" } } },
            { "RRC",    new Dictionary<ushort, string> { {(ushort)OppCodes.RRC ,    "B2" } } },
            { "PUSH",   new Dictionary<ushort, string> { {(ushort)OppCodes.PUSH ,   "B2" } } },
            { "POP",    new Dictionary<ushort, string> { {(ushort)OppCodes.POP ,    "B2" } } },

            { "BNE",    new Dictionary<ushort, string> { {(ushort)OppCodes.BNE ,    "B3" } } },
            { "BEQ",    new Dictionary<ushort, string> { {(ushort)OppCodes.BEQ ,    "B3" } } },
            { "BPL",    new Dictionary<ushort, string> { {(ushort)OppCodes.BPL ,    "B3" } } },
            { "BMI",    new Dictionary<ushort, string> { {(ushort)OppCodes.BMI ,    "B3" } } },
            { "BCS",    new Dictionary<ushort, string> { {(ushort)OppCodes.BCS ,    "B3" } } },
            { "BCC",    new Dictionary<ushort, string> { {(ushort)OppCodes.BCC ,    "B3" } } },
            { "BVS",    new Dictionary<ushort, string> { {(ushort)OppCodes.BVS ,    "B3" } } },
            { "BVC",    new Dictionary<ushort, string> { {(ushort)OppCodes.BVC ,    "B3" } } },
            { "JMP",    new Dictionary<ushort, string> { {(ushort)OppCodes.JMP ,    "B2" } } },
            { "CALL",   new Dictionary<ushort, string> { {(ushort)OppCodes.CALL ,   "B2" } } },

            { "CLC",    new Dictionary<ushort, string> { {(ushort)OppCodes.CLC ,    "B4" } } },
            { "SEC",    new Dictionary<ushort, string> { {(ushort)OppCodes.SEC ,    "B4" } } },
            { "NOP",    new Dictionary<ushort, string> { {(ushort)OppCodes.NOP ,    "B4" } } },
            { "HALT",   new Dictionary<ushort, string> { {(ushort)OppCodes.HALT ,   "B4" } } },
            { "EI",     new Dictionary<ushort, string> { {(ushort)OppCodes.EI ,     "B4" } } },
            { "DI",     new Dictionary<ushort, string> { {(ushort)OppCodes.DI ,     "B4" } } },
            { "PUSHPC", new Dictionary<ushort, string> { {(ushort)OppCodes.PUSHPC , "B4" } } },
            { "POPPC",  new Dictionary<ushort, string> { {(ushort)OppCodes.POPPC ,  "B4" } } },
            { "PUSHF",  new Dictionary<ushort, string> { {(ushort)OppCodes.PUSHF ,  "B4" } } },
            { "POPF",   new Dictionary<ushort, string> { {(ushort)OppCodes.POPF ,   "B4" } } },
            { "RET",    new Dictionary<ushort, string> { {(ushort)OppCodes.RET ,    "B4" } } },
            { "RETI",   new Dictionary<ushort, string> { {(ushort)OppCodes.RETI ,   "B4" } } },

        };
        readonly Dictionary<string, ushort> registers = new Dictionary<string, ushort>
        {
            { "R0",  0x0 },
            { "R1",  0x1 },
            { "R2",  0x2 },
            { "R3",  0x3 },
            { "R4",  0x4 },
            { "R5",  0x5 },
            { "R6",  0x6 },
            { "R7",  0x7 },
            { "R8",  0x8 },
            { "R9",  0x9 },
            { "R10", 0xA },
            { "R11", 0xB },
            { "R12", 0Xc },
            { "R13", 0xD },
            { "R14", 0xE },
            { "R15", 0xF },
        };
        bool _debug = false;
        internal byte[] Encode(ParseTreeNode node, out int len, bool _debug = false)
        {
            ResetEncoder();
            this._debug = _debug;
            ConstructSymbolTabel(node.ChildNodes[0]);
            TranverseInstructionList(node);
            len = _programIndex;
            return _program;
        }

        private void ResetEncoder()
        {
            for(int i=0;i<300;i++)
            {
                _program[i] = 0;
            }
            _instructionParts = default;
            _instrAddress = 0x0000;
            _symbolAddress = 0x0000;
            _programIndex = 0x0000;
        }

        private void ConstructSymbolTabel(ParseTreeNode node)
        {
            if (node == null) return;

            ushort previousAddress = 0;
            foreach (var buf in node.ChildNodes)
            {
                var child = buf.ChildNodes[0];
                if (child.Term.Name == "B1Instr" ||
                    child.Term.Name == "B2Instr" ||
                    child.Term.Name == "B3Instr" ||
                    child.Term.Name == "B4Instr")
                {
                    IncrementInstructionAddress(child);
                    DebugSymbols[(short)(previousAddress)] = (ushort)(child.Span.Location.Line + 1);
                }
                else if (child.Term.Name == "Label")
                {
                    HandleLabel(child);
                }
                else if (child.Term.Name == "Proc")
                {
                    HandleProc(child);
                }
                else if (child.Term.Name == "Instructions")
                {
                    TranverseInstructionList(child);
                }
                previousAddress = _symbolAddress;
            }
        }
        private void IncrementInstructionAddress(ParseTreeNode node)
        {
            switch(node.Term.Name)
            {
                case "B1Instr":
                case "B2Instr":
                case "B3Instr":
                    {
                        VerifyOperands(node);
                        _symbolAddress += 2;
                        break;
                    }
                case "B4Instr":
                    {
                        _symbolAddress += 2;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        private void VerifyOperands(ParseTreeNode node)
        {
            if(
                node.Term.Name == "B1Operand1" ||
                node.Term.Name == "B1Operand2" ||
                node.Term.Name == "B2Operand"  ||
                node.Term.Name == "B3Operand"
            )
            {
                if(node.ChildNodes[0].Term.Name == "MemoryAccess")
                {
                    VerifyMemoryAccess(node.ChildNodes[0]);
                }
                if(node.ChildNodes[0].Term.Name == "identifier" || (node.ChildNodes[0].Term.Name == "number"))
                {
                    _symbolAddress += 2;
                }

            }
            foreach(var childNode in node.ChildNodes)
            {
                VerifyOperands(childNode);
            }
        }
        private void VerifyMemoryAccess(ParseTreeNode node)
        {
            if (node.ChildNodes[0].Term.Name == "number?" && node.ChildNodes[0].ChildNodes.Count != 0)
            {
                _symbolAddress += 2;
            }
        }
        private void TranverseInstructionList(ParseTreeNode node)
        {
            if (node == null) return;

            foreach (var child in node.ChildNodes)
            {
                if (child.Term.Name == "Instr")
                {
                    HandleInstructions(child);
                }
                else
                {
                    TranverseInstructionList(child);
                }
            }
        }
        private void HandleInstructions(ParseTreeNode node)
        {
            var child = node.ChildNodes[0];
            Instruction instr = default;
            switch(child.Term.Name)
            {
                case "B1Instr":
                    {
                        // handleBxInstruction cannot return the parts of the instruction because
                        // the function is implemented recursively, insted a global variable is used.
                        HandleB1Instruction(child);
                        instr = AssembleInstruction(_instructionParts, 1);
                        break;
                    }

                case "B2Instr":
                    {
                        try
                        {
                            HandleB2Instruction(child);
                            instr = AssembleInstruction(_instructionParts, 2);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine($"Error: {e.Message}");
                        }
                        break;
                    }
                case "B3Instr":
                    {
                        try
                        {
                            HandleB3Instruction(child);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine($"Error: {e.Message}");
                        }
                        instr = AssembleInstruction(_instructionParts, 3);
                        break;
                    }
                case "B4Instr":
                    {
                        HandleB4Instruction(child);
                        instr = AssembleInstruction(_instructionParts, 4);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            StoreAssembledInstruction(instr);
        }

        /**
        *   Input: The node that points to the "Instr" Non-termianl
        *   Output: The parts of the instruction stored in _instructionParts
        */
        private void HandleB1Instruction(ParseTreeNode node)
        {
            var parent = node;
            switch (node.Term.Name)
            {
                case "B1Oppcodes":
                {
                    _instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    _instructionParts.Oppcode = _oppcodes[oppcode].Keys.First();
                    if(_debug) Console.Write(_instrAddress+" "+ oppcode);
                    return;
                }
                case "B1Operand1":
                {
                    CheckB1Operand1(node.ChildNodes[0]);
                    return;
                }
                case "B1Operand2":
                {
                    CheckB1Operand2(node.ChildNodes[0]);
                    return;
                }
                default:
                {
                    break;
                }
            }
            foreach(var childNode in parent.ChildNodes)
            {
                HandleB1Instruction(childNode);
            }
        }

        /**
        *   Input: The node that points to the "Instr" Non-termianl
        *   Output: The parts of the instruction stored in _instructionParts
        */
        private void HandleB2Instruction(ParseTreeNode node)
        {
            var parent = node;
            // add the oppcode to the _instructionParts
            switch (node.Term.Name)
            {
                case "B2Oppcodes":
                {
                    _instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    if(_debug) Console.Write(_instrAddress+" "+oppcode);
                    _instructionParts.Oppcode = _oppcodes[oppcode].Keys.First();
                    return;
                }
                case "B2Operand":
                {
                    CheckB2Operand(node.ChildNodes[0]);
                    return;
                }
                default:
                {
                    break;
                }
            }
            foreach(var childNode in parent.ChildNodes)
            {
                HandleB2Instruction(childNode);
            }
        }

        /**
        *   Input: The node that points to the "Instr" Non-termianl
        *   Output: The parts of the instruction stored in _instructionParts
        */
        private void HandleB3Instruction(ParseTreeNode node)
        {
            var parent = node;
            switch (node.Term.Name)
            {
                case "B3Oppcodes":
                {
                    _instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    if(_debug) Console.Write(_instrAddress+" "+oppcode);
                    _instructionParts.Oppcode = _oppcodes[oppcode].Keys.First();
                    return;
                }
                case "B3Operand":
                {
                    CheckB3Operand(node.ChildNodes[0]);
                    return;
                }
                default:
                {
                    break;
                }
            }
            foreach(var childNode in parent.ChildNodes)
            {
                HandleB3Instruction(childNode);
            }
        }

        /**
        *   Input: The node that points to the "Instr" Non-termianl
        *   Output: The parts of the instruction stored in _instructionPartS
        */
        private void HandleB4Instruction(ParseTreeNode node)
        {
            _instructionParts = default;
            string oppcode = node.ChildNodes[0].ChildNodes[0].Token.Text.ToUpper();
            if(_debug) Console.WriteLine(_instrAddress+" "+oppcode);
            _instructionParts.Oppcode = _oppcodes[oppcode].Keys.First();
            _instrAddress += 2;
        }
        private void CheckB3Operand(ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
                case "number":
                {
                    _instructionParts.Offset = Convert.ToInt16(node.Token.Value);
                    // add the register to the _instructionPartS
                    if(_debug) Console.WriteLine(" " + node.Token.Value);
                    _instrAddress += 2;
                    return;
                }
                case "identifier":
                {
                    string label = node.Token.Text;
                    // add the register to the _instructionPartS
                    if (!SymbolTable.ContainsKey(label))
                    {
                        throw new ArgumentException($"Unknown label: {label}");
                    }
                    if(_debug) Console.WriteLine(" " + label + " " + (SymbolTable[label] - _instrAddress));
                    _instructionParts.Offset = Convert.ToInt16(SymbolTable[label] - _instrAddress);
                    _instrAddress += 2;
                    return;
                }
            }
        }
        private void CheckB2Operand(ParseTreeNode node)
        {
            switch(node.Term.Name)
            {
                case "MemoryAccess":
                {
                    HandleMemoryAccess(node, ref _instructionParts.Mad, ref _instructionParts.Rd, ref _instructionParts.Offset1);
                    if(_debug) Console.WriteLine();
                    _instrAddress += 2;

                    return;
                }
                case "Register":
                {
                    string regiser = node.ChildNodes[0].Token.Text.ToUpper();
                    _instructionParts.Mad = 0b01;
                    _instructionParts.Rd = registers[regiser];
                    // add the register to the _instructionPartS
                    if(_debug) Console.WriteLine(" " + node.ChildNodes[0].Token.Text);
                    _instrAddress += 2;
                    return;
                }
                case "number":
                {
                    _instructionParts.Offset1 = Convert.ToInt16(node.Token.Value);
                    // add the register to the _instructionPartS
                    if(_debug)
                    {

                        Console.WriteLine(" " + node.Token.Value);
                        Console.WriteLine((_instrAddress + 2) + " " + node.Token.Value);
                    }
                    _instrAddress += 4;
                    return;
                }
                case "identifier":
                {
                    string label = node.Token.Text;
                    if (!SymbolTable.ContainsKey(label))
                    {
                        throw new ArgumentException($"Unknown label: {label}");
                    }
                    _instructionParts.Offset1 = Convert.ToInt16(SymbolTable[label]);
                    // add the register to the _instructionParts
                    if(_debug) Console.WriteLine(" " + label + " " + SymbolTable[label]);
                    _instrAddress += 4;
                    return;
                }
            }
        }
        private void CheckB1Operand2(ParseTreeNode node)
        {
            switch(node.Term.Name)
            {
                case "MemoryAccess":
                {
                    HandleMemoryAccess(node, ref _instructionParts.Mas, ref _instructionParts.Rs, ref _instructionParts.Offset2);
                    if(_debug) Console.WriteLine();
                    _instrAddress += 2;
                    return;
                }
                case "Register":
                {
                    // add the register to the _instructionParts
                    string regiser = node.ChildNodes[0].Token.Text.ToUpper();
                    _instructionParts.Mas = 0b01;
                    _instructionParts.Rs = registers[regiser];
                    if(_debug) Console.WriteLine(" " + node.ChildNodes[0].Token.Text);
                    _instrAddress += 2;
                    return;
                }
                case "number":
                {
                    // add the register to the _instrAddress_instructionPartS
                    if (_debug)
                    {
                        Console.WriteLine(" " + node.Token.Value);
                        Console.WriteLine(_instrAddress + 2 + " " + node.Token.Value);
                    }
                    _instructionParts.Offset2 = Convert.ToInt16(node.Token.Value);
                    _instrAddress += 4;
                    return;
                }
            }
        }
        private void CheckB1Operand1(ParseTreeNode node)
        {
            switch(node.Term.Name)
            {
                case "MemoryAccess":
                {
                    HandleMemoryAccess(node, ref _instructionParts.Mad, ref _instructionParts.Rd, ref _instructionParts.Offset1);
                    return;
                }
                case "Register":
                {
                    // add the register to the _instructionParts
                    string regiser = node.ChildNodes[0].Token.Text.ToUpper();
                    _instructionParts.Mad = 0b01;
                    _instructionParts.Rd = registers[regiser];
                    if(_debug) Console.Write(" " + node.ChildNodes[0].Token.Text);
                    return;
                }
            }
        }
        private void HandleMemoryAccess(ParseTreeNode node,ref ushort am,ref ushort reg, ref short offset)
        {
            switch (node.Term.Name)
            {
                case "number?":
                {
                    if (node.ChildNodes.Count != 0)
                    {
                        if(_debug) Console.Write(" " + node.ChildNodes[0].Token.Value);
                        am = 0b11;
                        offset = Convert.ToInt16(node.ChildNodes[0].Token.Value);
                        _instrAddress += 2;
                    }
                    return;
                }
                case "Register":
                {
                    if(am != 0b11)
                        am = 0b10;
                    string register = node.ChildNodes[0].Token.Text.ToUpper();
                    if(_debug) Console.Write(" " + node.ChildNodes[0].Token.Text);
                    reg = registers[register];
                    return;
                }
                default:
                {
                    break;
                }
            }
            foreach(var childNode in node.ChildNodes)
            {
                HandleMemoryAccess(childNode, ref am, ref reg, ref offset);
            }
        }
        private void HandleLabel(ParseTreeNode child)
        {
            string symbol = child.ChildNodes[0].Token.Text;
            SymbolTable[symbol] = _symbolAddress;
            if(_debug) Console.WriteLine(_symbolAddress + " " + child.ChildNodes[0].Token.Text);
        }
        private void HandleProc(ParseTreeNode child)
        {
            string symbol = child.ChildNodes[1].Token.Text;
            SymbolTable[symbol] = _symbolAddress;
            if(_debug) Console.WriteLine(_symbolAddress + " " + child.ChildNodes[1].Token.Text);
        }
        private Instruction AssembleInstruction(InstructionParts parts, ushort type)
        {
            switch(type)
            {
                case 1:
                    {
                        return AssembleB1(parts);
                    }
                case 2:
                    {
                        return AssembleB2(parts);
                    }
                case 3:
                    {
                        return AssembleB3(parts);
                    }
                case 4:
                    {
                        return AssembleB4(parts);
                    }
            }
            return default;
        }
        private Instruction AssembleB1(InstructionParts parts)
        {
            Instruction instr = default;
            instr.Instr   = (ushort)(parts.Oppcode | (parts.Mas << 10) | (parts.Rs << 6) | (parts.Mad << 4) | parts.Rd);
            instr.Offset1 = parts.Offset1;
            instr.Offset2 = parts.Offset2;
            return instr;
        }
        private Instruction AssembleB2(InstructionParts parts)
        {
            Instruction instr = default;
            instr.Instr   = (ushort)(parts.Oppcode | (parts.Mad << 4) | parts.Rd);
            instr.Offset1 = (short)(parts.Offset1 + s_programStartingAddress);
            instr.Offset2 = (short)(parts.Offset2 + s_programStartingAddress);
            return instr;
        }
        private Instruction AssembleB3(InstructionParts parts)
        {
            Instruction instr = default;
            instr.Instr = (ushort)(parts.Oppcode | ((ushort)parts.Offset & (ushort)0xFF));
            return instr;
        }
        private Instruction AssembleB4(InstructionParts parts)
        {
            Instruction instr = default;
            instr.Instr = parts.Oppcode;
            return instr;
        }
        private void StoreAssembledInstruction(Instruction instruction)
        {
            if(instruction.Instr != 0)
            {
                // Little endian representation - the MSB at the low byte
                _program[_programIndex++] = (byte)instruction.Instr;
                _program[_programIndex++] = (byte)(instruction.Instr >> 8);
            }
            if(instruction.Offset2 != 0)
            {
                _program[_programIndex++] = (byte)instruction.Offset2;
                _program[_programIndex++] = (byte)(instruction.Offset2>>8);
            }
            if(instruction.Offset1 != 0)
            {
                _program[_programIndex++] = (byte)instruction.Offset1;
                _program[_programIndex++] = (byte)(instruction.Offset1>>8);
            }
        }
    }
}

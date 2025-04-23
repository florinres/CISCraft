using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Assembler.Business
{
    internal class Encoder
    {
<<<<<<< HEAD
        static internal ushort programStartingAddress = 0;
=======
        InstructionParts _instructionParts = default;
        ushort _instrAddress  = 0x0000;
        ushort _symbolAddress = 0x0000;
        ushort _programIndex  = 0x0000;
        byte[] _program       = new byte[300];
        static internal ushort s_programStartingAddress { get; set; } = 0x0000;
>>>>>>> origin/feature/integrate_Assembler_with_UI

        /**
         * @brief  Enumerations for the CASM instructions
         * @note   The CASM instructions are divided into 4 types
         *       B1, B2, B3, B4
         * @details
         *      15 14 13                         0
         *      0  x  x  x x x x x x x x x x x x x
<<<<<<< HEAD
         *      ^ This marks that the instructionParts is of type B1
         *
         *      15 14 13                         0
         *      1  x  1  x x x x x x x x x x x x x
         *      ^ This marks that the instructionParts is of type B2
         *
         *      15 14 13                         0
         *      1  1  x  x x x x x x x x x x x x x
         *      ^ ^ This marks that the instructionParts is of type B3
         *
         *      15 14 13                         0
         *      1  1  1  x x x x x x x x x x x x x
         *      ^ ^ ^ This marks that the instructionParts is of type B4
         */
        enum OPPCODES
=======
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
>>>>>>> origin/feature/integrate_Assembler_with_UI
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
            NEG = 0xA040,
            INC = 0xA0C0,
            DEC = 0xA100,
            ASL = 0xA140,
            ASR = 0xA1C0,
            LSR = 0xA200,
            ROL = 0xA240,
            ROR = 0xA2C0,
            RLC = 0xA300,
            RRC = 0xA340,
            JMP = 0xA3C0,
            CALL = 0xA400,
            PUSH = 0xA440,
            POP = 0xA4C0,
            /** B3 INSTRUCTIONS TYPE
             * BR, BNE, BEQ, BPL, BMI, BCS, BCC, BVS, BVC
             * OPCODE: Operation Code - 8b
             * OFFSET: Offset - 8b
             * OPPCODE | OFFSET
             */
            BR = 0xC000, /* 0xC0 */
            BNE = 0xC100,
            BEQ = 0xC200,
            BPL = 0xC300,
            BMI = 0xC400,
            BCS = 0xC500,
            BCC = 0xC600,
            BVS = 0xC700,
            BVC = 0xC800,
            /** B4 INSTRUCTIONS TYPE
             * CLC, CLV, CLZ, CLS, CCC, SEC, SEV, SEZ, SES, SCC, NOP, RET, RETI, HALT,
             * WAIT, PUSHPC, POPPC, PUSHF, POPF
             * OPCODE: Operation Code - 16b
             * OPPCODE
             */
            CLC = 0xE000, /* 0xE00 */
            CLV,
            CLZ,
            CLS,
            CCC,
            SEC,
            SEV,
            SEZ,
            SES,
            SCC,
            NOP,
            RET,
            RETI,
            HALT,
            WAIT,
            PUSHPC,
            POPPC,
            PUSHF,
            POPF,
            /**
             * ERROR
             */
            ERR = 0xFFFF, /* 0xF00 */
        };
        struct InstructionParts
        {
<<<<<<< HEAD
            public ushort opcode;
            public ushort mas;
            public ushort mad;
            public ushort rs;
            public ushort rd;
            public short offset;
            public short offset1;
            public short offset2;
        }
        struct Instruction
        {
            public ushort instr;
            public short offset1;
            public short offset2;
        }
        InstructionParts instructionParts = default;
        ushort instrAddress  = 0x0000;
        ushort symbolAddress = 0x0000;
        ushort programIndex  = 0x0000;
        byte[] program       = new byte[300];
        Dictionary<string, ushort> symbolTable = new Dictionary<string, ushort>();
        Dictionary<string, Dictionary<ushort,string>> oppcodes = new Dictionary<string, Dictionary<ushort,string>>
        {
            // Numerical value    ,                                     Mnemonic, Type
            { "MOV",    new Dictionary<ushort, string> { {(ushort)OPPCODES.MOV ,    "B1" } } },
            { "ADD",    new Dictionary<ushort, string> { {(ushort)OPPCODES.ADD ,    "B1" } } },
            { "SUB",    new Dictionary<ushort, string> { {(ushort)OPPCODES.SUB ,    "B1" } } },
            { "CMP",    new Dictionary<ushort, string> { {(ushort)OPPCODES.CMP ,    "B1" } } },
            { "AND",    new Dictionary<ushort, string> { {(ushort)OPPCODES.AND ,    "B1" } } },
            { "OR" ,    new Dictionary<ushort, string> { {(ushort)OPPCODES.OR ,     "B1" } } },
            { "XOR",    new Dictionary<ushort, string> { {(ushort)OPPCODES.XOR ,    "B1" } } },

            { "CLR",    new Dictionary<ushort, string>{ {(ushort)OPPCODES.CLR ,     "B2" } } },
            { "NEG",    new Dictionary<ushort, string> { {(ushort)OPPCODES.NEG ,    "B2" } } },
            { "INC",    new Dictionary<ushort, string> { {(ushort)OPPCODES.INC ,    "B2" } } },
            { "DEC",    new Dictionary<ushort, string> { {(ushort)OPPCODES.DEC ,    "B2" } } },
            { "ASL",    new Dictionary<ushort, string> { {(ushort)OPPCODES.ASL ,    "B2" } } },
            { "ASR",    new Dictionary<ushort, string> { {(ushort)OPPCODES.ASR ,    "B2" } } },
            { "LSR",    new Dictionary<ushort, string> { {(ushort)OPPCODES.LSR ,    "B2" } } },
            { "ROL",    new Dictionary<ushort, string> { {(ushort)OPPCODES.ROL ,    "B2" } } },
            { "ROR",    new Dictionary<ushort, string> { {(ushort)OPPCODES.ROR ,    "B2" } } },
            { "RLC",    new Dictionary<ushort, string> { {(ushort)OPPCODES.RLC ,    "B2" } } },
            { "RRC",    new Dictionary<ushort, string> { {(ushort)OPPCODES.RRC ,    "B2" } } },
            { "JMP",    new Dictionary<ushort, string> { {(ushort)OPPCODES.JMP ,    "B2" } } },
            { "CALL",   new Dictionary<ushort, string> { {(ushort)OPPCODES.CALL ,   "B2" } } },
            { "PUSH",   new Dictionary<ushort, string> { {(ushort)OPPCODES.PUSH ,   "B2" } } },
            { "POP",    new Dictionary<ushort, string> { {(ushort)OPPCODES.POP ,    "B2" } } },

            { "BR",     new Dictionary<ushort, string> { {(ushort)OPPCODES.BR ,     "B3" } } },
            { "BNE",    new Dictionary<ushort, string> { {(ushort)OPPCODES.BNE ,    "B3" } } },
            { "BEQ",    new Dictionary<ushort, string> { {(ushort)OPPCODES.BEQ ,    "B3" } } },
            { "BPL",    new Dictionary<ushort, string> { {(ushort)OPPCODES.BPL ,    "B3" } } },
            { "BMI",    new Dictionary<ushort, string> { {(ushort)OPPCODES.BMI ,    "B3" } } },
            { "BCS",    new Dictionary<ushort, string> { {(ushort)OPPCODES.BCS ,    "B3" } } },
            { "BCC",    new Dictionary<ushort, string> { {(ushort)OPPCODES.BCC ,    "B3" } } },
            { "BVS",    new Dictionary<ushort, string> { {(ushort)OPPCODES.BVS ,    "B3" } } },
            { "BVC",    new Dictionary<ushort, string> { {(ushort)OPPCODES.BVC ,    "B3" } } },

            { "CLC",    new Dictionary<ushort, string> { {(ushort)OPPCODES.CLC ,    "B4" } } },
            { "CLV",    new Dictionary<ushort, string> { {(ushort)OPPCODES.CLV ,    "B4" } } },
            { "CLZ",    new Dictionary<ushort, string> { {(ushort)OPPCODES.CLZ ,    "B4" } } },
            { "CLS",    new Dictionary<ushort, string> { {(ushort)OPPCODES.CLS ,    "B4" } } },
            { "CCC",    new Dictionary<ushort, string> { {(ushort)OPPCODES.CCC ,    "B4" } } },
            { "SEC",    new Dictionary<ushort, string> { {(ushort)OPPCODES.SEC ,    "B4" } } },
            { "SEV",    new Dictionary<ushort, string> { {(ushort)OPPCODES.SEV ,    "B4" } } },
            { "SEZ",    new Dictionary<ushort, string> { {(ushort)OPPCODES.SEZ ,    "B4" } } },
            { "SES",    new Dictionary<ushort, string> { {(ushort)OPPCODES.SES ,    "B4" } } },
            { "SCC",    new Dictionary<ushort, string> { {(ushort)OPPCODES.SCC ,    "B4" } } },
            { "NOP",    new Dictionary<ushort, string> { {(ushort)OPPCODES.NOP ,    "B4" } } },
            { "RET",    new Dictionary<ushort, string> { {(ushort)OPPCODES.RET ,    "B4" } } },
            { "RETI",   new Dictionary<ushort, string> { {(ushort)OPPCODES.RETI ,   "B4" } } },
            { "HALT",   new Dictionary<ushort, string> { {(ushort)OPPCODES.HALT ,   "B4" } } },
            { "WAIT",   new Dictionary<ushort, string> { {(ushort)OPPCODES.WAIT ,   "B4" } } },
            { "PUSHPC", new Dictionary<ushort, string> { {(ushort)OPPCODES.PUSHPC , "B4" } } },
            { "POPPC",  new Dictionary<ushort, string> { {(ushort)OPPCODES.POPPC ,  "B4" } } },
            { "PUSHF",  new Dictionary<ushort, string> { {(ushort)OPPCODES.PUSHF ,  "B4" } } },
            { "POPF",   new Dictionary<ushort, string> { {(ushort)OPPCODES.POPF ,   "B4" } } },

        };
        Dictionary<string, ushort> registers = new Dictionary<string, ushort>
=======
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
        readonly Dictionary<string, ushort> _symbolTable = new Dictionary<string, ushort>();
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
            { "JMP",    new Dictionary<ushort, string> { {(ushort)OppCodes.JMP ,    "B2" } } },
            { "CALL",   new Dictionary<ushort, string> { {(ushort)OppCodes.CALL ,   "B2" } } },
            { "PUSH",   new Dictionary<ushort, string> { {(ushort)OppCodes.PUSH ,   "B2" } } },
            { "POP",    new Dictionary<ushort, string> { {(ushort)OppCodes.POP ,    "B2" } } },

            { "BR",     new Dictionary<ushort, string> { {(ushort)OppCodes.BR ,     "B3" } } },
            { "BNE",    new Dictionary<ushort, string> { {(ushort)OppCodes.BNE ,    "B3" } } },
            { "BEQ",    new Dictionary<ushort, string> { {(ushort)OppCodes.BEQ ,    "B3" } } },
            { "BPL",    new Dictionary<ushort, string> { {(ushort)OppCodes.BPL ,    "B3" } } },
            { "BMI",    new Dictionary<ushort, string> { {(ushort)OppCodes.BMI ,    "B3" } } },
            { "BCS",    new Dictionary<ushort, string> { {(ushort)OppCodes.BCS ,    "B3" } } },
            { "BCC",    new Dictionary<ushort, string> { {(ushort)OppCodes.BCC ,    "B3" } } },
            { "BVS",    new Dictionary<ushort, string> { {(ushort)OppCodes.BVS ,    "B3" } } },
            { "BVC",    new Dictionary<ushort, string> { {(ushort)OppCodes.BVC ,    "B3" } } },

            { "CLC",    new Dictionary<ushort, string> { {(ushort)OppCodes.CLC ,    "B4" } } },
            { "CLV",    new Dictionary<ushort, string> { {(ushort)OppCodes.CLV ,    "B4" } } },
            { "CLZ",    new Dictionary<ushort, string> { {(ushort)OppCodes.CLZ ,    "B4" } } },
            { "CLS",    new Dictionary<ushort, string> { {(ushort)OppCodes.CLS ,    "B4" } } },
            { "CCC",    new Dictionary<ushort, string> { {(ushort)OppCodes.CCC ,    "B4" } } },
            { "SEC",    new Dictionary<ushort, string> { {(ushort)OppCodes.SEC ,    "B4" } } },
            { "SEV",    new Dictionary<ushort, string> { {(ushort)OppCodes.SEV ,    "B4" } } },
            { "SEZ",    new Dictionary<ushort, string> { {(ushort)OppCodes.SEZ ,    "B4" } } },
            { "SES",    new Dictionary<ushort, string> { {(ushort)OppCodes.SES ,    "B4" } } },
            { "SCC",    new Dictionary<ushort, string> { {(ushort)OppCodes.SCC ,    "B4" } } },
            { "NOP",    new Dictionary<ushort, string> { {(ushort)OppCodes.NOP ,    "B4" } } },
            { "RET",    new Dictionary<ushort, string> { {(ushort)OppCodes.RET ,    "B4" } } },
            { "RETI",   new Dictionary<ushort, string> { {(ushort)OppCodes.RETI ,   "B4" } } },
            { "HALT",   new Dictionary<ushort, string> { {(ushort)OppCodes.HALT ,   "B4" } } },
            { "WAIT",   new Dictionary<ushort, string> { {(ushort)OppCodes.WAIT ,   "B4" } } },
            { "PUSHPC", new Dictionary<ushort, string> { {(ushort)OppCodes.PUSHPC , "B4" } } },
            { "POPPC",  new Dictionary<ushort, string> { {(ushort)OppCodes.POPPC ,  "B4" } } },
            { "PUSHF",  new Dictionary<ushort, string> { {(ushort)OppCodes.PUSHF ,  "B4" } } },
            { "POPF",   new Dictionary<ushort, string> { {(ushort)OppCodes.POPF ,   "B4" } } },

        };
        readonly Dictionary<string, ushort> registers = new Dictionary<string, ushort>
>>>>>>> origin/feature/integrate_Assembler_with_UI
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
<<<<<<< HEAD

        bool debug = false;
        internal byte[] Encode(ParseTreeNode node, out int len, bool debug = false)
        {
            this.debug = debug;
            ConstructSymbolTabel(node.ChildNodes[0]);
            TranverseInstructionList(node);
            len = programIndex;
            return program;
=======
        bool _debug = false;
        internal byte[] Encode(ParseTreeNode node, out int len, bool _debug = false)
        {
            this._debug = _debug;
            ConstructSymbolTabel(node.ChildNodes[0]);
            TranverseInstructionList(node);
            len = _programIndex;
            return _program;
>>>>>>> origin/feature/integrate_Assembler_with_UI
        }
        private void ConstructSymbolTabel(ParseTreeNode node)
        {
            if (node == null) return;

            foreach (var buf in node.ChildNodes)
            {
                var child = buf.ChildNodes[0];
                if (child.Term.Name == "B1Instr" ||
                    child.Term.Name == "B2Instr" ||
                    child.Term.Name == "B3Instr" ||
                    child.Term.Name == "B4Instr")
                {
<<<<<<< HEAD
                    incrementInstructionAddress(child);
                }
                else if (child.Term.Name == "Label")
                {
                    handleLabel(child);
                }
                else if (child.Term.Name == "Proc")
                {
                    handleProc(child);
=======
                    IncrementInstructionAddress(child);
                }
                else if (child.Term.Name == "Label")
                {
                    HandleLabel(child);
                }
                else if (child.Term.Name == "Proc")
                {
                    HandleProc(child);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                }
                else if (child.Term.Name == "Instructions")
                {
                    TranverseInstructionList(child);
                }
            }
        }
<<<<<<< HEAD
        private void incrementInstructionAddress(ParseTreeNode node)
=======
        private void IncrementInstructionAddress(ParseTreeNode node)
>>>>>>> origin/feature/integrate_Assembler_with_UI
        {
            switch(node.Term.Name)
            {
                case "B1Instr":
<<<<<<< HEAD
                    {
                        verifyOperands(node);
                        symbolAddress += 2;
                        break;
                    }

                case "B2Instr":
                    {
                        verifyOperands(node);
                        symbolAddress += 2;
=======
                case "B2Instr":
                    {
                        VerifyOperands(node);
                        _symbolAddress += 2;
>>>>>>> origin/feature/integrate_Assembler_with_UI
                        break;
                    }
                case "B3Instr":
                    {
<<<<<<< HEAD
                        symbolAddress += 2;
=======
                        _symbolAddress += 2;
>>>>>>> origin/feature/integrate_Assembler_with_UI
                        break;
                    }
                case "B4Instr":
                    {
<<<<<<< HEAD
                        symbolAddress += 2;
=======
                        _symbolAddress += 2;
>>>>>>> origin/feature/integrate_Assembler_with_UI
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
<<<<<<< HEAD
        private void verifyOperands(ParseTreeNode node)
        {
            if( 
=======
        private void VerifyOperands(ParseTreeNode node)
        {
            if(
>>>>>>> origin/feature/integrate_Assembler_with_UI
                node.Term.Name == "B1Operand1" ||
                node.Term.Name == "B1Operand2" ||
                node.Term.Name == "B2Operand"
            )
            {
                if(node.ChildNodes[0].Term.Name == "MemoryAccess")
                {
<<<<<<< HEAD
                    verifyMemoryAccess(node.ChildNodes[0]);
                }
                if(node.ChildNodes[0].Term.Name == "identifier" || (node.ChildNodes[0].Term.Name == "number" && node.Term.Name == "B2Operand"))
                {
                    symbolAddress += 2;
=======
                    VerifyMemoryAccess(node.ChildNodes[0]);
                }
                if(node.ChildNodes[0].Term.Name == "identifier" || (node.ChildNodes[0].Term.Name == "number" && node.Term.Name == "B2Operand"))
                {
                    _symbolAddress += 2;
>>>>>>> origin/feature/integrate_Assembler_with_UI
                }

            }
            foreach(var childNode in node.ChildNodes)
            {
<<<<<<< HEAD
                verifyOperands(childNode);
            }
        }
        private void verifyMemoryAccess(ParseTreeNode node)
        {
            if (node.ChildNodes[0].Term.Name == "number?" && node.ChildNodes[0].ChildNodes.Count != 0)
            {
                symbolAddress += 2;
=======
                VerifyOperands(childNode);
            }
        }
        private void VerifyMemoryAccess(ParseTreeNode node)
        {
            if (node.ChildNodes[0].Term.Name == "number?" && node.ChildNodes[0].ChildNodes.Count != 0)
            {
                _symbolAddress += 2;
>>>>>>> origin/feature/integrate_Assembler_with_UI
            }
        }
        private void TranverseInstructionList(ParseTreeNode node)
        {
            if (node == null) return;

            foreach (var child in node.ChildNodes)
            {
                if (child.Term.Name == "Instr")
                {
<<<<<<< HEAD
                    handleInstructions(child);
=======
                    HandleInstructions(child);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                }
                else
                {
                    TranverseInstructionList(child);
                }
            }
        }
<<<<<<< HEAD
        private void handleInstructions(ParseTreeNode node)
=======
        private void HandleInstructions(ParseTreeNode node)
>>>>>>> origin/feature/integrate_Assembler_with_UI
        {
            var child = node.ChildNodes[0];
            Instruction instr = default;
            switch(child.Term.Name)
            {
                case "B1Instr":
                    {
                        // handleBxInstruction cannot return the parts of the instruction because
                        // the function is implemented recursively, insted a global variable is used.
<<<<<<< HEAD
                        handleB1Instruction(child);
                        instr = assembleInstruction(instructionParts, 1);
=======
                        HandleB1Instruction(child);
                        instr = AssembleInstruction(_instructionParts, 1);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                        break;
                    }

                case "B2Instr":
                    {
                        try
                        {
<<<<<<< HEAD
                            handleB2Instruction(child);
                            instr = assembleInstruction(instructionParts, 2);
=======
                            HandleB2Instruction(child);
                            instr = AssembleInstruction(_instructionParts, 2);
>>>>>>> origin/feature/integrate_Assembler_with_UI
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
<<<<<<< HEAD
                            handleB3Instruction(child);
=======
                            HandleB3Instruction(child);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine($"Error: {e.Message}");
                        }
<<<<<<< HEAD
                        instr = assembleInstruction(instructionParts, 3);
=======
                        instr = AssembleInstruction(_instructionParts, 3);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                        break;
                    }
                case "B4Instr":
                    {
<<<<<<< HEAD
                        handleB4Instruction(child);
                        instr = assembleInstruction(instructionParts, 4);
=======
                        HandleB4Instruction(child);
                        instr = AssembleInstruction(_instructionParts, 4);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
<<<<<<< HEAD
            storeAssembledInstruction(instr);
=======
            StoreAssembledInstruction(instr);
>>>>>>> origin/feature/integrate_Assembler_with_UI
        }

        /**
        *   Input: The node that points to the "Instr" Non-termianl
<<<<<<< HEAD
        *   Output: The parts of the instruction stored in instructionParts
        */
        private void handleB1Instruction(ParseTreeNode node)
=======
        *   Output: The parts of the instruction stored in _instructionParts
        */
        private void HandleB1Instruction(ParseTreeNode node)
>>>>>>> origin/feature/integrate_Assembler_with_UI
        {
            var parent = node;
            switch (node.Term.Name)
            {
                case "B1Oppcodes":
                {
<<<<<<< HEAD
                    instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    instructionParts.opcode = oppcodes[oppcode].Keys.First();
                    if(debug) Console.Write(instrAddress+" "+ oppcode);
=======
                    _instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    _instructionParts.Oppcode = _oppcodes[oppcode].Keys.First();
                    if(_debug) Console.Write(_instrAddress+" "+ oppcode);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                    return;
                }
                case "B1Operand1":
                {
<<<<<<< HEAD
                    switch(parent.ChildNodes[0].Term.Name)
                    {
                        case "MemoryAccess":
                        {
                            handleMemoryAccess(parent.ChildNodes[0], ref instructionParts.mad, ref instructionParts.rd, ref instructionParts.offset1);
                            return;
                        }
                        case "Register":
                        {
                            // add the register to the instructionParts
                            string regiser = parent.ChildNodes[0].ChildNodes[0].Token.Text.ToUpper();
                            instructionParts.mad = 0b01;
                            instructionParts.rd = registers[regiser];
                            if(debug) Console.Write(" " + parent.ChildNodes[0].ChildNodes[0].Token.Text);
                            return;
                        }
                    }
=======
                    CheckB1Operand1(node.ChildNodes[0]);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                    return;
                }
                case "B1Operand2":
                {
<<<<<<< HEAD
                    switch(parent.ChildNodes[0].Term.Name)
                    {
                        case "MemoryAccess":
                        {
                            handleMemoryAccess(parent.ChildNodes[0], ref instructionParts.mas, ref instructionParts.rs, ref instructionParts.offset2);
                            if(debug) Console.WriteLine();
                            instrAddress += 2;
                            return;
                        }
                        case "Register":
                        {
                            // add the register to the instructionParts
                            string regiser = parent.ChildNodes[0].ChildNodes[0].Token.Text.ToUpper();
                            instructionParts.mas = 0b01;
                            instructionParts.rs = registers[regiser];
                            if(debug) Console.WriteLine(" " + parent.ChildNodes[0].ChildNodes[0].Token.Text);
                            instrAddress += 2;
                            return;
                        }
                        case "number":
                        {
                            // add the register to the instructionParts
                            if (debug)
                            {
                                Console.WriteLine(" " + parent.ChildNodes[0].Token.Value);
                                Console.WriteLine(instrAddress + 2 + " " + parent.ChildNodes[0].Token.Value);
                            }
                            instructionParts.offset2 = Convert.ToInt16(parent.ChildNodes[0].Token.Value);
                            instrAddress += 4;
                            return;
                        }
                    }
=======
                    CheckB1Operand2(node.ChildNodes[0]);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                    return;
                }
                default:
                {
                    break;
                }
            }
            foreach(var childNode in parent.ChildNodes)
            {
<<<<<<< HEAD
                handleB1Instruction(childNode);
=======
                HandleB1Instruction(childNode);
>>>>>>> origin/feature/integrate_Assembler_with_UI
            }
        }

        /**
        *   Input: The node that points to the "Instr" Non-termianl
<<<<<<< HEAD
        *   Output: The parts of the instruction stored in instructionParts
        */
        private void handleB2Instruction(ParseTreeNode node)
        {
            var parent = node;
            // add the oppcode to the instructionParts;
=======
        *   Output: The parts of the instruction stored in _instructionParts
        */
        private void HandleB2Instruction(ParseTreeNode node)
        {
            var parent = node;
            // add the oppcode to the _instructionParts
>>>>>>> origin/feature/integrate_Assembler_with_UI
            switch (node.Term.Name)
            {
                case "B2Oppcodes":
                {
<<<<<<< HEAD
                    instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    if(debug) Console.Write(instrAddress+" "+oppcode);
                    instructionParts.opcode = oppcodes[oppcode].Keys.First();
=======
                    _instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    if(_debug) Console.Write(_instrAddress+" "+oppcode);
                    _instructionParts.Oppcode = _oppcodes[oppcode].Keys.First();
>>>>>>> origin/feature/integrate_Assembler_with_UI
                    return;
                }
                case "B2Operand":
                {
<<<<<<< HEAD
                    switch(parent.ChildNodes[0].Term.Name)
                    {
                        case "MemoryAccess":
                        {
                            handleMemoryAccess(parent.ChildNodes[0], ref instructionParts.mad, ref instructionParts.rd, ref instructionParts.offset1);
                            if(debug) Console.WriteLine();
                            instrAddress += 2;

                            return;
                        }
                        case "Register":
                        {
                            string regiser = parent.ChildNodes[0].ChildNodes[0].Token.Text.ToUpper();
                            instructionParts.mad = 0b01;
                            instructionParts.rd = registers[regiser];
                            // add the register to the instructionParts
                            if(debug) Console.WriteLine(" " + parent.ChildNodes[0].ChildNodes[0].Token.Text);
                            instrAddress += 2;
                            return;
                        }
                        case "number":
                        {
                            instructionParts.offset1 = Convert.ToInt16(parent.ChildNodes[0].Token.Value);
                            // add the register to the instructionParts
                            if(debug)
                            {

                                Console.WriteLine(" " + parent.ChildNodes[0].Token.Value);
                                Console.WriteLine((instrAddress + 2) + " " + parent.ChildNodes[0].Token.Value);
                            }
                            instrAddress += 4;
                            return;
                        }
                        case "identifier":
                        {
                            string label = parent.ChildNodes[0].Token.Text;
                            if (!symbolTable.ContainsKey(label))
                            {
                                throw new Exception($"Unknown label: {label}");
                            }
                            instructionParts.offset1 = Convert.ToInt16(symbolTable[label]);
                            // add the register to the instructionParts
                            if(debug) Console.WriteLine(" " + label + " " + symbolTable[label]);
                            instrAddress += 4;
                            return;
                        }
                    }
=======
                    CheckB2Operand(node.ChildNodes[0]);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                    return;
                }
                default:
                {
                    break;
                }
            }
            foreach(var childNode in parent.ChildNodes)
            {
<<<<<<< HEAD
                handleB2Instruction(childNode);
=======
                HandleB2Instruction(childNode);
>>>>>>> origin/feature/integrate_Assembler_with_UI
            }
        }

        /**
<<<<<<< HEAD
        *   Input: The node that points to the "Instr" Non-termianl 
        *   Output: The parts of the instruction stored in instructionParts
        */
        private void handleB3Instruction(ParseTreeNode node)
=======
        *   Input: The node that points to the "Instr" Non-termianl
        *   Output: The parts of the instruction stored in _instructionParts
        */
        private void HandleB3Instruction(ParseTreeNode node)
>>>>>>> origin/feature/integrate_Assembler_with_UI
        {
            var parent = node;
            switch (node.Term.Name)
            {
                case "B3Oppcodes":
                {
<<<<<<< HEAD
                    instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    if(debug) Console.Write(instrAddress+" "+oppcode);
                    instructionParts.opcode = oppcodes[oppcode].Keys.First();
                    return;
                }
                case "B3Operand":
                { 
                    switch (parent.ChildNodes[0].Term.Name)
                    {
                        case "number":
                        {
                            instructionParts.offset = Convert.ToInt16(parent.ChildNodes[0].Token.Value);
                            // add the register to the instructionParts
                            if(debug) Console.WriteLine(" " + parent.ChildNodes[0].Token.Value);
                            instrAddress += 2;
                            return;
                        }
                        case "identifier":
                        {
                            string label = parent.ChildNodes[0].Token.Text;
                            // add the register to the instructionParts
                            if (!symbolTable.ContainsKey(label))
                            {
                                throw new Exception($"Unknown label: {label}");
                            }
                            if(debug) Console.WriteLine(" " + label + " " + (symbolTable[label] - instrAddress));
                            instructionParts.offset = Convert.ToInt16(symbolTable[label] - instrAddress);
                            instrAddress += 2;
                            return;
                        }
                    } 
=======
                    _instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    if(_debug) Console.Write(_instrAddress+" "+oppcode);
                    _instructionParts.Oppcode = _oppcodes[oppcode].Keys.First();
                    return;
                }
                case "B3Operand":
                {
                    CheckB3Operand(node.ChildNodes[0]);
>>>>>>> origin/feature/integrate_Assembler_with_UI
                    return;
                }
                default:
                {
                    break;
                }
            }
            foreach(var childNode in parent.ChildNodes)
            {
<<<<<<< HEAD
                handleB3Instruction(childNode);
=======
                HandleB3Instruction(childNode);
>>>>>>> origin/feature/integrate_Assembler_with_UI
            }
        }

        /**
<<<<<<< HEAD
        *   Input: The node that points to the "Instr" Non-termianl 
        *   Output: The parts of the instruction stored in instructionParts
        */
        private void handleB4Instruction(ParseTreeNode node)
        {
            instructionParts = default;
            string oppcode = node.ChildNodes[0].ChildNodes[0].Token.Text.ToUpper();
            if(debug) Console.WriteLine(instrAddress+" "+oppcode);
            instructionParts.opcode = oppcodes[oppcode].Keys.First();
            instrAddress += 2;
        }
        private void handleMemoryAccess(ParseTreeNode node,ref ushort am,ref ushort reg, ref short offset)
=======
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
                    if (!_symbolTable.ContainsKey(label))
                    {
                        throw new ArgumentException($"Unknown label: {label}");
                    }
                    if(_debug) Console.WriteLine(" " + label + " " + (_symbolTable[label] - _instrAddress));
                    _instructionParts.Offset = Convert.ToInt16(_symbolTable[label] - _instrAddress);
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
                    if (!_symbolTable.ContainsKey(label))
                    {
                        throw new ArgumentException($"Unknown label: {label}");
                    }
                    _instructionParts.Offset1 = Convert.ToInt16(_symbolTable[label]);
                    // add the register to the _instructionParts
                    if(_debug) Console.WriteLine(" " + label + " " + _symbolTable[label]);
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
>>>>>>> origin/feature/integrate_Assembler_with_UI
        {
            switch (node.Term.Name)
            {
                case "number?":
                {
                    if (node.ChildNodes.Count != 0)
                    {
<<<<<<< HEAD
                        if(debug) Console.Write(" " + node.ChildNodes[0].Token.Value);
                        am = 0b11;
                        offset = Convert.ToInt16(node.ChildNodes[0].Token.Value);
                        instrAddress += 2;
=======
                        if(_debug) Console.Write(" " + node.ChildNodes[0].Token.Value);
                        am = 0b11;
                        offset = Convert.ToInt16(node.ChildNodes[0].Token.Value);
                        _instrAddress += 2;
>>>>>>> origin/feature/integrate_Assembler_with_UI
                    }
                    return;
                }
                case "Register":
                {
                    if(am != 0b11)
                        am = 0b10;
                    string register = node.ChildNodes[0].Token.Text.ToUpper();
<<<<<<< HEAD
                    if(debug) Console.Write(" " + node.ChildNodes[0].Token.Text);
=======
                    if(_debug) Console.Write(" " + node.ChildNodes[0].Token.Text);
>>>>>>> origin/feature/integrate_Assembler_with_UI
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
<<<<<<< HEAD
                handleMemoryAccess(childNode, ref am, ref reg, ref offset);
            }
        }
        private void handleLabel(ParseTreeNode child)
        {
            string symbol = child.ChildNodes[0].Token.Text;
            symbolTable[symbol] = symbolAddress;
            if(debug) Console.WriteLine(symbolAddress + " " + child.ChildNodes[0].Token.Text);
        }
        private void handleProc(ParseTreeNode child)
        {
            string symbol = child.ChildNodes[1].Token.Text;
            symbolTable[symbol] = symbolAddress;
            if(debug) Console.WriteLine(symbolAddress + " " + child.ChildNodes[1].Token.Text);
        }
        private Instruction assembleInstruction(InstructionParts parts, ushort type)
=======
                HandleMemoryAccess(childNode, ref am, ref reg, ref offset);
            }
        }
        private void HandleLabel(ParseTreeNode child)
        {
            string symbol = child.ChildNodes[0].Token.Text;
            _symbolTable[symbol] = _symbolAddress;
            if(_debug) Console.WriteLine(_symbolAddress + " " + child.ChildNodes[0].Token.Text);
        }
        private void HandleProc(ParseTreeNode child)
        {
            string symbol = child.ChildNodes[1].Token.Text;
            _symbolTable[symbol] = _symbolAddress;
            if(_debug) Console.WriteLine(_symbolAddress + " " + child.ChildNodes[1].Token.Text);
        }
        private Instruction AssembleInstruction(InstructionParts parts, ushort type)
>>>>>>> origin/feature/integrate_Assembler_with_UI
        {
            switch(type)
            {
                case 1:
                    {
<<<<<<< HEAD
                        return assembleB1(parts);
                    }
                case 2:
                    {
                        return assembleB2(parts);
                    }
                case 3:
                    {
                        return assembleB3(parts);
                    }
                case 4:
                    {
                        return assembleB4(parts);
=======
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
>>>>>>> origin/feature/integrate_Assembler_with_UI
                    }
            }
            return default;
        }
<<<<<<< HEAD
        private Instruction assembleB1(InstructionParts parts)
        {
            Instruction instr = default;
            instr.instr   = (ushort)(parts.opcode | (parts.mas << 10) | (parts.rs << 6) | (parts.mad << 4) | parts.rd);
            instr.offset1 = parts.offset1;
            instr.offset2 = parts.offset2;
            return instr;
        }
        private Instruction assembleB2(InstructionParts parts)
        {
            Instruction instr = default;
            instr.instr   = (ushort)(parts.opcode | (parts.mad << 4) | parts.rd);
            instr.offset1 = (short)(parts.offset1 + programStartingAddress);
            instr.offset2 = (short)(parts.offset2 + programStartingAddress);
            return instr;
        }
        private Instruction assembleB3(InstructionParts parts)
        {
            Instruction instr = default;
            instr.instr = (ushort)(parts.opcode | ((ushort)parts.offset & (ushort)0xFF));
            return instr;
        }
        private Instruction assembleB4(InstructionParts parts)
        {
            Instruction instr = default;
            instr.instr = (ushort)(parts.opcode);
            return instr;
        }
        private void storeAssembledInstruction(Instruction instruction)
        {
            if(instruction.instr != 0)
            {
                // Little endian representation - the MSB at the low byte
                program[programIndex++] = (byte)(instruction.instr>>8);
                program[programIndex++] = (byte)instruction.instr;
            }
            if(instruction.offset1 != 0)
            {
                program[programIndex++] = (byte)(instruction.offset1>>8);
                program[programIndex++] = (byte)instruction.offset1;
            }
            if(instruction.offset2 != 0)
            {
                program[programIndex++] = (byte)(instruction.offset2>>8);
                program[programIndex++] = (byte)instruction.offset2;
=======
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
                _program[_programIndex++] = (byte)(instruction.Instr>>8);
                _program[_programIndex++] = (byte)instruction.Instr;
            }
            if(instruction.Offset1 != 0)
            {
                _program[_programIndex++] = (byte)(instruction.Offset1>>8);
                _program[_programIndex++] = (byte)instruction.Offset1;
            }
            if(instruction.Offset2 != 0)
            {
                _program[_programIndex++] = (byte)(instruction.Offset2>>8);
                _program[_programIndex++] = (byte)instruction.Offset2;
>>>>>>> origin/feature/integrate_Assembler_with_UI
            }
        }
    }
}

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
        /**
         * @brief  Enumerations for the CASM instructions
         * @note   The CASM instructions are divided into 4 types
         *       B1, B2, B3, B4
         * @details
         *      15 14 13                         0
         *      0  x  x  x x x x x x x x x x x x x
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
        ushort instrAddress  = 0x0000;
        ushort symbolAddress = 0x0000;
        struct InstructionParts
        {
            public ushort opcode;
            public ushort mas;
            public ushort mad;
            public ushort rs;
            public ushort rd;
            public short offset;
            public short offset1;
            public short offset2;
        }
        InstructionParts instructionParts;
        struct Instruction
        {
            public ushort instr;
            public short offset1;
            public short offset2;
        }
        byte[] program = new byte[300];
        int programIndex = 0;
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
        internal byte[] Encode(ParseTreeNode node, out int len)
        {
            ConstructSymbolTabel(node.ChildNodes[0]);
            TranverseInstructionList(node);
            len = programIndex;
            return program;
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
                    incrementInstructionAddress(child);
                }
                else if (child.Term.Name == "Label")
                {
                    handleLabel(child);
                }
                else if (child.Term.Name == "Proc")
                {
                    handleProc(child);
                }
                else if (child.Term.Name == "Instructions")
                {
                    TranverseInstructionList(child);
                }
            }
        }
        private void incrementInstructionAddress(ParseTreeNode node)
        {
            switch(node.Term.Name)
            {
                case "B1Instr":
                    {
                        verifyOperands(node);
                        symbolAddress += 2;
                        break;
                    }

                case "B2Instr":
                    {
                        verifyOperands(node);
                        symbolAddress += 2;
                        break;
                    }
                case "B3Instr":
                    {
                        symbolAddress += 2;
                        break;
                    }
                case "B4Instr":
                    {
                        symbolAddress += 2;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        private void verifyOperands(ParseTreeNode node)
        {
            if( 
                node.Term.Name == "B1Operand1" ||
                node.Term.Name == "B1Operand2" ||
                node.Term.Name == "B2Operand"
            )
            {
                if(node.ChildNodes[0].Term.Name == "MemoryAccess")
                {
                    verifyMemoryAccess(node.ChildNodes[0]);
                }
                if(node.ChildNodes[0].Term.Name == "identifier" || (node.ChildNodes[0].Term.Name == "number" && node.Term.Name == "B2Operand"))
                {
                    symbolAddress += 2;
                }

            }
            foreach(var childNode in node.ChildNodes)
            {
                verifyOperands(childNode);
            }
        }
        private void verifyMemoryAccess(ParseTreeNode node)
        {
            if (node.ChildNodes[0].Term.Name == "number?" && node.ChildNodes[0].ChildNodes.Count != 0)
            {
                symbolAddress += 2;
            }
        }
        private void TranverseInstructionList(ParseTreeNode node)
        {
            if (node == null) return;

            foreach (var child in node.ChildNodes)
            {
                if (child.Term.Name == "Instr")
                {
                    handleInstructions(child);
                }
                else
                {
                    TranverseInstructionList(child);
                }
            }
        }
        private void handleInstructions(ParseTreeNode node)
        {
            var child = node.ChildNodes[0];
            Instruction instr = default;
            switch(child.Term.Name)
            {
                case "B1Instr":
                    {
                        // handleBxInstruction cannot return the parts of the instruction because
                        // the function is implemented recursively, insted a global variable is used.
                        handleB1Instruction(child);
                        instr = assembleInstruction(instructionParts, 1);
                        break;
                    }

                case "B2Instr":
                    {
                        handleB2Instruction(child);
                        instr = assembleInstruction(instructionParts, 2);
                        break;
                    }
                case "B3Instr":
                    {
                        handleB3Instruction(child);
                        instr = assembleInstruction(instructionParts, 3);
                        break;
                    }
                case "B4Instr":
                    {
                        handleB4Instruction(child);
                        instr = assembleInstruction(instructionParts, 4);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            storeAssembledInstruction(instr);
        }

        /**
        *   Input: The node that points to the "Instr" Non-termianl
        *   Output: The parts of the instruction stored in instructionParts
        */
        private void handleB1Instruction(ParseTreeNode node)
        {
            var parent = node;
            switch (node.Term.Name)
            {
                case "B1Oppcodes":
                {
                    instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    Console.Write(instrAddress+" "+oppcode);
                    instructionParts.opcode = oppcodes[oppcode].Keys.First();
                    return;
                }
                case "B1Operand1":
                {
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
                            Console.Write(" " + parent.ChildNodes[0].ChildNodes[0].Token.Text);
                            return;
                        }
                    }
                    return;
                }
                case "B1Operand2":
                {
                    switch(parent.ChildNodes[0].Term.Name)
                    {
                        case "MemoryAccess":
                        {
                            handleMemoryAccess(parent.ChildNodes[0], ref instructionParts.mas, ref instructionParts.rs, ref instructionParts.offset2);
                            Console.WriteLine();
                            instrAddress += 2;
                            return;
                        }
                        case "Register":
                        {
                            // add the register to the instructionParts
                            string regiser = parent.ChildNodes[0].ChildNodes[0].Token.Text.ToUpper();
                            instructionParts.mas = 0b01;
                            instructionParts.rs = registers[regiser];
                            Console.WriteLine(" " + parent.ChildNodes[0].ChildNodes[0].Token.Text);
                            instrAddress += 2;
                            return;
                        }
                        case "number":
                        {
                            // add the register to the instructionParts
                            Console.WriteLine(" " + parent.ChildNodes[0].Token.Value);
                            Console.WriteLine(instrAddress + 2 + " " + parent.ChildNodes[0].Token.Value);
                            instructionParts.offset2 = Convert.ToInt16(parent.ChildNodes[0].Token.Value);
                            instrAddress += 4;
                            return;
                        }
                    }
                    return;
                }
                default:
                {
                    break;
                }
            }
            foreach(var childNode in parent.ChildNodes)
            {
                handleB1Instruction(childNode);
            }
        }

        /**
        *   Input: The node that points to the "Instr" Non-termianl
        *   Output: The parts of the instruction stored in instructionParts
        */
        private void handleB2Instruction(ParseTreeNode node)
        {
            var parent = node;
            // add the oppcode to the instructionParts;
            switch (node.Term.Name)
            {
                case "B2Oppcodes":
                {
                    instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    Console.Write(instrAddress+" "+oppcode);
                    instructionParts.opcode = oppcodes[oppcode].Keys.First();
                    return;
                }
                case "B2Operand":
                {
                    switch(parent.ChildNodes[0].Term.Name)
                    {
                        case "MemoryAccess":
                        {
                            handleMemoryAccess(parent.ChildNodes[0], ref instructionParts.mad, ref instructionParts.rd, ref instructionParts.offset1);
                            Console.WriteLine();
                            instrAddress += 2;

                            return;
                        }
                        case "Register":
                        {
                            string regiser = parent.ChildNodes[0].ChildNodes[0].Token.Text.ToUpper();
                            instructionParts.mad = 0b01;
                            instructionParts.rd = registers[regiser];
                            // add the register to the instructionParts
                            Console.WriteLine(" " + parent.ChildNodes[0].ChildNodes[0].Token.Text);
                            instrAddress += 2;
                            return;
                        }
                        case "number":
                        {
                            instructionParts.offset1 = Convert.ToInt16(parent.ChildNodes[0].Token.Value);
                            // add the register to the instructionParts
                            Console.WriteLine(" " + parent.ChildNodes[0].Token.Value);
                            Console.WriteLine((instrAddress + 2) + " " + parent.ChildNodes[0].Token.Value);
                            instrAddress += 4;
                            return;
                        }
                        case "identifier":
                        {
                            string label = parent.ChildNodes[0].Token.Text;
                            instructionParts.offset1 = Convert.ToInt16(symbolTable[label]);
                            // add the register to the instructionParts
                            Console.WriteLine(" " + label + " " + symbolTable[label]);
                            instrAddress += 4;
                            return;
                        }
                    }
                    return;
                }
                default:
                {
                    break;
                }
            }
            foreach(var childNode in parent.ChildNodes)
            {
                handleB2Instruction(childNode);
            }
        }

        /**
        *   Input: The node that points to the "Instr" Non-termianl 
        *   Output: The parts of the instruction stored in instructionParts
        */
        private void handleB3Instruction(ParseTreeNode node)
        {
            var parent = node;
            switch (node.Term.Name)
            {
                case "B3Oppcodes":
                {
                    instructionParts = default;
                    string oppcode = node.ChildNodes[0].Token.Text.ToUpper();
                    Console.Write(instrAddress+" "+oppcode);
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
                            Console.WriteLine(" " + parent.ChildNodes[0].Token.Value);
                            instrAddress += 2;
                            return;
                        }
                        case "identifier":
                        {
                            string symbol = parent.ChildNodes[0].Token.Text;
                            // add the register to the instructionParts
                            if (symbolTable.ContainsKey(symbol))
                            {
                                Console.Write(" " + symbolTable[symbol]);
                            }
                            Console.WriteLine(" " + symbol + " " + (symbolTable[symbol] - instrAddress));
                            instructionParts.offset = Convert.ToInt16(symbolTable[symbol] - instrAddress);
                            instrAddress += 2;
                            return;
                        }
                    } 
                    return;
                }
                default:
                {
                    break;
                }
            }
            foreach(var childNode in parent.ChildNodes)
            {
                handleB3Instruction(childNode);
            }
        }

        /**
        *   Input: The node that points to the "Instr" Non-termianl 
        *   Output: The parts of the instruction stored in instructionParts
        */
        private void handleB4Instruction(ParseTreeNode node)
        {
            instructionParts = default;
            string oppcode = node.ChildNodes[0].ChildNodes[0].Token.Text.ToUpper();
            Console.WriteLine(instrAddress+" "+oppcode);
            instructionParts.opcode = oppcodes[oppcode].Keys.First();
            instrAddress += 2;
        }
        private void handleMemoryAccess(ParseTreeNode node,ref ushort am,ref ushort reg, ref short offset)
        {
            switch (node.Term.Name)
            {
                case "number?":
                {
                    if (node.ChildNodes.Count != 0)
                    {
                        Console.Write(" " + node.ChildNodes[0].Token.Value);
                        am = 0b11;
                        offset = Convert.ToInt16(node.ChildNodes[0].Token.Value);
                        instrAddress += 2;
                    }
                    return;
                }
                case "Register":
                {
                    if(am != 0b11)
                        am = 0b10;
                    string register = node.ChildNodes[0].Token.Text.ToUpper();
                    Console.Write(" " + node.ChildNodes[0].Token.Text);
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
                handleMemoryAccess(childNode, ref am, ref reg, ref offset);
            }
        }
        private void handleLabel(ParseTreeNode child)
        {
            string symbol = child.ChildNodes[0].Token.Text;
            symbolTable[symbol] = symbolAddress;
            Console.WriteLine(symbolAddress + " " + child.ChildNodes[0].Token.Text);
        }
        private void handleProc(ParseTreeNode child)
        {
            string symbol = child.ChildNodes[1].Token.Text;
            symbolTable[symbol] = symbolAddress;
            Console.WriteLine(symbolAddress + " " + child.ChildNodes[1].Token.Text);
        }
        private Instruction assembleInstruction(InstructionParts parts, ushort type)
        {
            switch(type)
            {
                case 1:
                    {
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
                    }
            }
            return default;
        }
        private Instruction assembleB1(InstructionParts parts)
        {
            Instruction instr = default;
            instr.instr = (ushort)(parts.opcode | (parts.mas << 10) | (parts.rs << 6) | (parts.mad << 4) | parts.rd);
            instr.offset1 = parts.offset1;
            instr.offset2 = parts.offset2;
            return instr;
        }
        private Instruction assembleB2(InstructionParts parts)
        {
            Instruction instr = default;
            instr.instr = (ushort)(parts.opcode | (parts.mad << 4) | parts.rd);
            instr.offset1 = parts.offset1;    
            instr.offset2 = parts.offset2;
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
            }
        }
    }
}

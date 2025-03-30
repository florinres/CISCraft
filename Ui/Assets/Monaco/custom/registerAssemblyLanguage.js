monaco.languages.register({ id: 'assembly' });

monaco.languages.setMonarchTokensProvider('assembly', {
    keywords: [
        'mov', 'add', 'sub', 'mul', 'div', 'xor', 'and', 'or', 'cmp', 'jmp', 'je', 'jne', 'call', 'ret',
        'push', 'pop', 'lea', 'nop', 'int'
    ],
    registers: [
        'eax', 'ebx', 'ecx', 'edx', 'esi', 'edi', 'esp', 'ebp',
        'ax', 'bx', 'cx', 'dx', 'si', 'di', 'sp', 'bp',
        'al', 'ah', 'bl', 'bh', 'cl', 'ch', 'dl', 'dh'
    ],
    tokenizer: {
        root: [
            [/[;.].*/, 'comment'],  // Comments start with ; or .
            [/\b(?:[0-9]+|0x[0-9a-fA-F]+|0b[01]+)\b/, 'number'], // Numbers
            [/\b(?:EAX|EBX|ECX|EDX|ESI|EDI|ESP|EBP|AX|BX|CX|DX|SI|DI|SP|BP|AL|AH|BL|BH|CL|CH|DL|DH)\b/i, 'variable'], // Registers (case-insensitive)
            [/\b(?:MOV|ADD|SUB|MUL|DIV|XOR|AND|OR|CMP|JMP|JE|JNE|CALL|RET|PUSH|POP|LEA|NOP|INT)\b/i, 'keyword'], // Instructions (case-insensitive)
            [/".*?"/, 'string'], // Strings
            [/'.*?'/, 'string'], // Chars
            [/\b[a-zA-Z_]\w*:/, 'label'], // Labels (word followed by :)
            [/[a-zA-Z_]\w*/, 'identifier'] // Labels and other identifiers
        ]
    }
});

; Example of useful procedures written for MicroCISC

mov r0,4
mov r1,10
mov r2,8
mov r3,5
call factorial
halt

; Input: R0,R1
; Output: Swaps the contents of R0 and R1
proc swap

    mov r2,r1
    mov r1,r0
    mov r0,r2
    ret
endp swap

; Input: R2
; Output: Computes 2^R2 (power of 2)
proc pow2

    mov r3,1
	dec r2
    pow2_loop:
    asl r3  ; Shifts R3 to the left with one position
    cmp r2,0
    beq pow2_exit
    dec r2
    jmp pow2_loop

    pow2_exit:
    mov r2,r3 ; Moves result back to R2
    ret

endp pow2

; Input: R0,R1
; Output: R0 = R0 * R1
proc mul

    cmp r1, 0   ; Check if multiplier is zero
    beq case_0_mul

    cmp r1, 1
    beq mul_exit

    mov r2,r0   ; Preserve original value of R0
    dec r1

    mul_loop:
    add r0,r2
    dec r1
    cmp r1, 0
    bne mul_loop
    jmp mul_exit

    case_0_mul:
    mov r0,0

    mul_exit:
    ret

; Input: R3
; Output: Computes R3 factorial
proc factorial

    cmp r3,0
    beq case_0

    mov r6, r3 ; Store original state
    mov r5, r3  ; Store final result
    mov r4, r3 ; Store index

    factorial_loop:
    mov r3, r6 ; Restore state of R3
    dec r4
    push r0 ; Save values into stack
    push r1

    mov r0, r5
    sub r3, r4 ; n-index
    mov r1, r3

    call mul
    mov r5,r0
    pop r1
    pop r0

    cmp r4, 1
    bne factorial_loop
    mov r3, r5 ; Store final result back in R3
    jmp factorial_end

    case_0:
    mov r3, 1 ; 0!= 1
    jmp factorial_end

    factorial_end:
	ret
endp factorial
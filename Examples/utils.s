; Example of useful procedures written for MicroCISC

mov r0,4
mov r1,-1
call swap
mov r2,5
call pow2

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
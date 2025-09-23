; Default example code for MicroCISC

ei ; Enables interrupts

mov r0,5
mov r1,3
call mul
halt ; Halts program execution by stopping CPU clock

; Input: R0,R1
; Output: R0 = R0 * R1
proc mul

    cmp r1, 0   ; Check if multiplier is zero
    beq mul_exit

    mov r2,r0   ; Preserve original value of R0
    dec r1

    mul_loop:
    add r0,r2
    dec r1
    cmp r1, 0
    bne mul_loop

    mul_exit:
    ret

endp mul
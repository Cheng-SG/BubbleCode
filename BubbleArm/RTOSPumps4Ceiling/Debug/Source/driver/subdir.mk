################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../Source/driver/adc.c \
../Source/driver/can.c \
../Source/driver/clkconfig.c \
../Source/driver/crp.c \
../Source/driver/debug_printf.c \
../Source/driver/gpio.c \
../Source/driver/i2c.c \
../Source/driver/i2cslave.c \
../Source/driver/lpc_swu.c \
../Source/driver/rs485.c \
../Source/driver/small_gpio.c \
../Source/driver/ssp.c \
../Source/driver/timer16.c \
../Source/driver/timer32.c \
../Source/driver/uart.c \
../Source/driver/wdt.c 

OBJS += \
./Source/driver/adc.o \
./Source/driver/can.o \
./Source/driver/clkconfig.o \
./Source/driver/crp.o \
./Source/driver/debug_printf.o \
./Source/driver/gpio.o \
./Source/driver/i2c.o \
./Source/driver/i2cslave.o \
./Source/driver/lpc_swu.o \
./Source/driver/rs485.o \
./Source/driver/small_gpio.o \
./Source/driver/ssp.o \
./Source/driver/timer16.o \
./Source/driver/timer32.o \
./Source/driver/uart.o \
./Source/driver/wdt.o 

C_DEPS += \
./Source/driver/adc.d \
./Source/driver/can.d \
./Source/driver/clkconfig.d \
./Source/driver/crp.d \
./Source/driver/debug_printf.d \
./Source/driver/gpio.d \
./Source/driver/i2c.d \
./Source/driver/i2cslave.d \
./Source/driver/lpc_swu.d \
./Source/driver/rs485.d \
./Source/driver/small_gpio.d \
./Source/driver/ssp.d \
./Source/driver/timer16.d \
./Source/driver/timer32.d \
./Source/driver/uart.d \
./Source/driver/wdt.d 


# Each subdirectory must supply rules for building sources it contributes
Source/driver/%.o: ../Source/driver/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -D__REDLIB__ -DDEBUG -D__CODE_RED -D__USE_CMSIS=CMSISv2p00_LPC11xx -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSPumps4Ceiling/Source/driver" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/CMSISv2p00_LPC11xx/inc" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSPumps4Ceiling/Source" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSPumps4Ceiling/Source/FreeRTOS_Source/include" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSPumps4Ceiling/Source/FreeRTOS_Source/portable/GCC/ARM_CM0" -O0 -g3 -Wall -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -Wextra -mcpu=cortex-m0 -mthumb -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '



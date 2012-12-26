################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../src/Ceiling_main.c \
../src/Control.c \
../src/Flowrate.c \
../src/Pumps.c \
../src/UartPacket.c 

OBJS += \
./src/Ceiling_main.o \
./src/Control.o \
./src/Flowrate.o \
./src/Pumps.o \
./src/UartPacket.o 

C_DEPS += \
./src/Ceiling_main.d \
./src/Control.d \
./src/Flowrate.d \
./src/Pumps.d \
./src/UartPacket.d 


# Each subdirectory must supply rules for building sources it contributes
src/%.o: ../src/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -D__REDLIB__ -D__USE_CMSIS -DDEBUG -D__CODE_RED -I../cmsis -I../config -I../driver -O0 -g3 -Wall -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -mcpu=cortex-m0 -mthumb -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '



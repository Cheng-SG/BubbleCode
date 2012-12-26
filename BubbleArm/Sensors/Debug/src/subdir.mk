################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../src/Sensors_main.c \
../src/UartPacket.c 

OBJS += \
./src/Sensors_main.o \
./src/UartPacket.o 

C_DEPS += \
./src/Sensors_main.d \
./src/UartPacket.d 


# Each subdirectory must supply rules for building sources it contributes
src/%.o: ../src/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -D__REDLIB__ -D__USE_CMSIS -DDEBUG -D__CODE_RED -I"/Users/vivid/Dropbox/My files/code/BubbleArm/Sensors/config" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/Sensors/cmsis" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/Sensors/driver" -O0 -g3 -Wall -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -mcpu=cortex-m0 -mthumb -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '



################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../src/Flowrate.c \
../src/Pumps.c \
../src/Pumps_main.c 

OBJS += \
./src/Flowrate.o \
./src/Pumps.o \
./src/Pumps_main.o 

C_DEPS += \
./src/Flowrate.d \
./src/Pumps.d \
./src/Pumps_main.d 


# Each subdirectory must supply rules for building sources it contributes
src/%.o: ../src/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -D__REDLIB__ -I"/Users/vivid/Dropbox/My files/code/BubbleArm/Pumps/config" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/Pumps/cmsis" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/Pumps/driver" -O1 -Os -g -Wall -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -mcpu=cortex-m0 -mthumb -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -o"$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '



################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../Source/Control.c \
../Source/Flowrate.c \
../Source/Pumps.c \
../Source/UartPacket.c \
../Source/cr_startup_lpc11.c \
../Source/main.c 

OBJS += \
./Source/Control.o \
./Source/Flowrate.o \
./Source/Pumps.o \
./Source/UartPacket.o \
./Source/cr_startup_lpc11.o \
./Source/main.o 

C_DEPS += \
./Source/Control.d \
./Source/Flowrate.d \
./Source/Pumps.d \
./Source/UartPacket.d \
./Source/cr_startup_lpc11.d \
./Source/main.d 


# Each subdirectory must supply rules for building sources it contributes
Source/%.o: ../Source/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -D__REDLIB__ -DDEBUG -D__CODE_RED -D__USE_CMSIS=CMSISv2p00_LPC11xx -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSAirboxSupplyControl/Source/driver" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/CMSISv2p00_LPC11xx/inc" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSAirboxSupplyControl/Source" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSAirboxSupplyControl/Source/FreeRTOS_Source/include" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSAirboxSupplyControl/Source/FreeRTOS_Source/portable/GCC/ARM_CM0" -O0 -g3 -Wall -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -Wextra -mcpu=cortex-m0 -mthumb -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '

Source/cr_startup_lpc11.o: ../Source/cr_startup_lpc11.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -D__REDLIB__ -DDEBUG -D__CODE_RED -D__USE_CMSIS=CMSISv2p00_LPC11xx -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSAirboxSupplyControl/Source/driver" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/CMSISv2p00_LPC11xx/inc" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSAirboxSupplyControl/Source" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSAirboxSupplyControl/Source/FreeRTOS_Source/include" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSAirboxSupplyControl/Source/FreeRTOS_Source/portable/GCC/ARM_CM0" -O0 -Os -g3 -Wall -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -Wextra -mcpu=cortex-m0 -mthumb -MMD -MP -MF"$(@:%.o=%.d)" -MT"Source/cr_startup_lpc11.d" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '



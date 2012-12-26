################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../Source/IntQueueTimer.c \
../Source/RegTest.c \
../Source/cr_startup_lpc11.c \
../Source/main-blinky.c \
../Source/main-full.c \
../Source/main.c 

OBJS += \
./Source/IntQueueTimer.o \
./Source/RegTest.o \
./Source/cr_startup_lpc11.o \
./Source/main-blinky.o \
./Source/main-full.o \
./Source/main.o 

C_DEPS += \
./Source/IntQueueTimer.d \
./Source/RegTest.d \
./Source/cr_startup_lpc11.d \
./Source/main-blinky.d \
./Source/main-full.d \
./Source/main.d 


# Each subdirectory must supply rules for building sources it contributes
Source/%.o: ../Source/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -D__REDLIB__ -DDEBUG -D__CODE_RED -D__USE_CMSIS=CMSISv2p00_LPC11xx -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSDemo/Source/Common_Demo_Task/include" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/CMSISv2p00_LPC11xx/inc" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSDemo/Source" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSDemo/Source/FreeRTOS_Source/include" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSDemo/Source/FreeRTOS_Source/portable/GCC/ARM_CM0" -O0 -g3 -Wall -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -Wextra -mcpu=cortex-m0 -mthumb -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '

Source/cr_startup_lpc11.o: ../Source/cr_startup_lpc11.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -D__REDLIB__ -DDEBUG -D__CODE_RED -D__USE_CMSIS=CMSISv2p00_LPC11xx -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSDemo/Source/Common_Demo_Task/include" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/CMSISv2p00_LPC11xx/inc" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSDemo/Source" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSDemo/Source/FreeRTOS_Source/include" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSDemo/Source/FreeRTOS_Source/portable/GCC/ARM_CM0" -O0 -Os -g3 -Wall -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -Wextra -mcpu=cortex-m0 -mthumb -MMD -MP -MF"$(@:%.o=%.d)" -MT"Source/cr_startup_lpc11.d" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '



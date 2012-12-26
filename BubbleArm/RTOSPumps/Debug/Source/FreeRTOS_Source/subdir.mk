################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../Source/FreeRTOS_Source/croutine.c \
../Source/FreeRTOS_Source/list.c \
../Source/FreeRTOS_Source/queue.c \
../Source/FreeRTOS_Source/tasks.c \
../Source/FreeRTOS_Source/timers.c 

OBJS += \
./Source/FreeRTOS_Source/croutine.o \
./Source/FreeRTOS_Source/list.o \
./Source/FreeRTOS_Source/queue.o \
./Source/FreeRTOS_Source/tasks.o \
./Source/FreeRTOS_Source/timers.o 

C_DEPS += \
./Source/FreeRTOS_Source/croutine.d \
./Source/FreeRTOS_Source/list.d \
./Source/FreeRTOS_Source/queue.d \
./Source/FreeRTOS_Source/tasks.d \
./Source/FreeRTOS_Source/timers.d 


# Each subdirectory must supply rules for building sources it contributes
Source/FreeRTOS_Source/%.o: ../Source/FreeRTOS_Source/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -D__REDLIB__ -DDEBUG -D__CODE_RED -D__USE_CMSIS=CMSISv2p00_LPC11xx -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSPumps/Source/driver" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/CMSISv2p00_LPC11xx/inc" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSPumps/Source" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSPumps/Source/FreeRTOS_Source/include" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOSPumps/Source/FreeRTOS_Source/portable/GCC/ARM_CM0" -O0 -g3 -Wall -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -Wextra -mcpu=cortex-m0 -mthumb -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '



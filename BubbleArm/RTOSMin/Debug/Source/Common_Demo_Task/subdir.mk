################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../Source/Common_Demo_Task/IntQueue.c \
../Source/Common_Demo_Task/blocktim.c \
../Source/Common_Demo_Task/countsem.c \
../Source/Common_Demo_Task/recmutex.c 

OBJS += \
./Source/Common_Demo_Task/IntQueue.o \
./Source/Common_Demo_Task/blocktim.o \
./Source/Common_Demo_Task/countsem.o \
./Source/Common_Demo_Task/recmutex.o 

C_DEPS += \
./Source/Common_Demo_Task/IntQueue.d \
./Source/Common_Demo_Task/blocktim.d \
./Source/Common_Demo_Task/countsem.d \
./Source/Common_Demo_Task/recmutex.d 


# Each subdirectory must supply rules for building sources it contributes
Source/Common_Demo_Task/%.o: ../Source/Common_Demo_Task/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -D__REDLIB__ -DDEBUG -D__CODE_RED -D__USE_CMSIS=CMSISv2p00_LPC11xx -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOS_Sensors/Source/Common_Demo_Task/include" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOS_Sensors/Source/driver" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/CMSISv2p00_LPC11xx/inc" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOS_Sensors/Source" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOS_Sensors/Source/FreeRTOS_Source/include" -I"/Users/vivid/Dropbox/My files/code/BubbleArm/RTOS_Sensors/Source/FreeRTOS_Source/portable/GCC/ARM_CM0" -O0 -g3 -Wall -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -Wextra -mcpu=cortex-m0 -mthumb -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '



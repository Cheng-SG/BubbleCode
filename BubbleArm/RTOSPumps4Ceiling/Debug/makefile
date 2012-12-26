################################################################################
# Automatically-generated file. Do not edit!
################################################################################

-include ../makefile.init

RM := rm -rf

# All of the sources participating in the build are defined here
-include sources.mk
-include Source/driver/subdir.mk
-include Source/FreeRTOS_Source/portable/MemMang/subdir.mk
-include Source/FreeRTOS_Source/portable/GCC/ARM_CM0/subdir.mk
-include Source/FreeRTOS_Source/subdir.mk
-include Source/subdir.mk
-include subdir.mk
-include objects.mk

ifneq ($(MAKECMDGOALS),clean)
ifneq ($(strip $(C_DEPS)),)
-include $(C_DEPS)
endif
endif

-include ../makefile.defs

# Add inputs and outputs from these tool invocations to the build variables 

# All Target
all: RTOSPumps4Ceiling.axf

# Tool invocations
RTOSPumps4Ceiling.axf: $(OBJS) $(USER_OBJS)
	@echo 'Building target: $@'
	@echo 'Invoking: MCU Linker'
	arm-none-eabi-gcc -nostdlib -L"/Users/vivid/Dropbox/My files/code/BubbleArm/CMSISv2p00_LPC11xx/Debug" -Xlinker -Map=RTOSPumps4Ceiling.map -Xlinker --gc-sections -mcpu=cortex-m0 -mthumb -T "RTOSPumps4Ceiling_Debug.ld" -o "RTOSPumps4Ceiling.axf" $(OBJS) $(USER_OBJS) $(LIBS)
	@echo 'Finished building target: $@'
	@echo ' '
	$(MAKE) --no-print-directory post-build

# Other Targets
clean:
	-$(RM) $(OBJS)$(C_DEPS)$(EXECUTABLES) RTOSPumps4Ceiling.axf
	-@echo ' '

post-build:
	-@echo 'Performing post-build steps'
	-arm-none-eabi-size RTOSPumps4Ceiling.axf; # arm-none-eabi-objcopy -O binary RTOSPumps4Ceiling.axf RTOSPumps4Ceiling.bin ; checksum -p LPC1113_301 -d RTOSPumps4Ceiling.bin;
	-@echo ' '

.PHONY: all clean dependents
.SECONDARY: post-build

-include ../makefile.targets

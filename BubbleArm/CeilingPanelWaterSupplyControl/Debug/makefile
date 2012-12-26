################################################################################
# Automatically-generated file. Do not edit!
################################################################################

-include ../makefile.init

RM := rm -rf

# All of the sources participating in the build are defined here
-include sources.mk
-include startup/subdir.mk
-include src/subdir.mk
-include driver/subdir.mk
-include cmsis/subdir.mk
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
all: CeilingPanelWaterSupplyControl.axf

# Tool invocations
CeilingPanelWaterSupplyControl.axf: $(OBJS) $(USER_OBJS)
	@echo 'Building target: $@'
	@echo 'Invoking: MCU Linker'
	arm-none-eabi-gcc -nostdlib -Xlinker -Map=CeilingPanelWaterSupplyControl.map -Xlinker --gc-sections -mcpu=cortex-m0 -mthumb -T "../linker/lpc1114_flash_crp.ld" -o "CeilingPanelWaterSupplyControl.axf" $(OBJS) $(USER_OBJS) $(LIBS)
	@echo 'Finished building target: $@'
	@echo ' '
	$(MAKE) --no-print-directory post-build

# Other Targets
clean:
	-$(RM) $(OBJS)$(C_DEPS)$(EXECUTABLES) CeilingPanelWaterSupplyControl.axf
	-@echo ' '

post-build:
	-@echo 'Performing post-build steps'
	-arm-none-eabi-size CeilingPanelWaterSupplyControl.axf; arm-none-eabi-objcopy -O binary CeilingPanelWaterSupplyControl.axf CeilingPanelWaterSupplyControl.bin ; checksum -p LPC1113_301 -d CeilingPanelWaterSupplyControl.bin;
	-@echo ' '

.PHONY: all clean dependents
.SECONDARY: post-build

-include ../makefile.targets

/*
 * Pumps.c
 *
 *  Created on: Apr 2, 2012
 *      Author: vivid
 */

#include "driver_config.h"
#include "target_config.h"

#include "type.h"
#include "i2c.h"
#include "gpio.h"
#include "Pumps.h"

extern volatile uint8_t I2CMasterBuffer[I2C_BUFSIZE];
extern volatile uint8_t I2CSlaveBuffer[I2C_BUFSIZE];
extern volatile uint32_t I2CMasterState;
extern volatile uint32_t I2CReadLength, I2CWriteLength;

volatile uint16_t PumpsSpeed[8];

uint32_t PumpsInit()
{
	/* Initialize GPIO (sets up clock) */
	GPIOInit();
	/* Set pumps port pin to output */
	GPIOSetDir(PUMPS_PORT, PUMPS_1_BIT, 1);
	LPC_IOCON->R_PIO1_0 = 0x0009;
	GPIOSetValue(PUMPS_PORT, PUMPS_1_BIT, 0);

	GPIOSetDir(PUMPS_PORT, PUMPS_2_BIT, 1);
	LPC_IOCON->R_PIO1_1 = 0x0009;
	GPIOSetValue(PUMPS_PORT, PUMPS_2_BIT, 0);

	GPIOSetDir(PUMPS_PORT, PUMPS_3_BIT, 1);
	LPC_IOCON->PIO1_4 = 0x0008;
	GPIOSetValue(PUMPS_PORT, PUMPS_3_BIT, 0);

	GPIOSetDir(PUMPS_PORT, PUMPS_4_BIT, 1);
	LPC_IOCON->PIO1_5 = 0x0008;
	GPIOSetValue(PUMPS_PORT, PUMPS_4_BIT, 0);

	GPIOSetDir(PUMPS_PORT, PUMPS_5_BIT, 1);
	LPC_IOCON->PIO1_8 = 0x0008;
	GPIOSetValue(PUMPS_PORT, PUMPS_5_BIT, 0);

	GPIOSetDir(PUMPS_PORT, PUMPS_6_BIT, 1);
	LPC_IOCON->PIO1_9 = 0x0008;
	GPIOSetValue(PUMPS_PORT, PUMPS_6_BIT, 0);

	GPIOSetDir(PUMPS_PORT, PUMPS_7_BIT, 1);
	LPC_IOCON->PIO1_10 = 0x0008;
	GPIOSetValue(PUMPS_PORT, PUMPS_7_BIT, 0);

	GPIOSetDir(PUMPS_PORT, PUMPS_8_BIT, 1);
	LPC_IOCON->PIO1_11 = 0x0008;
	GPIOSetValue(PUMPS_PORT, PUMPS_8_BIT, 0);

	if (I2CInit((uint32_t) I2CMASTER) == FALSE) /* initialize I2c */
	{
		while (1)
			; /* Fatal error */
	}

	PumpsSpeed[0] = 0xFFFF;
	PumpsSpeed[1] = 0xFFFF;
	PumpsSpeed[2] = 0xFFFF;
	PumpsSpeed[3] = 0xFFFF;
	PumpsSpeed[4] = 0xFFFF;
	PumpsSpeed[5] = 0xFFFF;
	PumpsSpeed[6] = 0xFFFF;
	PumpsSpeed[7] = 0xFFFF;

	I2CWriteLength = 4;
	I2CReadLength = 0;
	I2CMasterBuffer[0] = ((AD5669R_ADDR) << 1);
	I2CMasterBuffer[1] = 0x3F; /* address */
	I2CMasterBuffer[2] = 0xFF;
	I2CMasterBuffer[3] = 0xFF;
	I2CEngine();
	if (I2CMasterState != I2C_OK)
	{
		return FALSE;
	}
	return TRUE;
}

uint32_t PumpsSetSpeed(uint8_t channel, uint16_t value)
{
	if (!(channel >= 1 && channel <= 8))
		return FALSE;
	channel -= 1;
	I2CWriteLength = 4;
	I2CReadLength = 0;
	I2CMasterBuffer[0] = ((AD5669R_ADDR) << 1);
	I2CMasterBuffer[1] = (0x30 + channel); /* address */
	I2CMasterBuffer[2] = (value >> 8);
	I2CMasterBuffer[3] = value;
	I2CEngine();
	if (I2CMasterState != I2C_OK)
	{
		return FALSE;
	}
	if (value > 62259)
	{
		switch (channel)
		{
		case 0:
			GPIOSetValue(PUMPS_PORT, PUMPS_1_BIT, 0);
			break;
		case 1:
			GPIOSetValue(PUMPS_PORT, PUMPS_2_BIT, 0);
			break;
		case 2:
			GPIOSetValue(PUMPS_PORT, PUMPS_3_BIT, 0);
			break;
		case 3:
			GPIOSetValue(PUMPS_PORT, PUMPS_4_BIT, 0);
			break;
		case 4:
			GPIOSetValue(PUMPS_PORT, PUMPS_5_BIT, 0);
			break;
		case 5:
			GPIOSetValue(PUMPS_PORT, PUMPS_6_BIT, 0);
			break;
		case 6:
			GPIOSetValue(PUMPS_PORT, PUMPS_7_BIT, 0);
			break;
		case 7:
			GPIOSetValue(PUMPS_PORT, PUMPS_8_BIT, 0);
			break;
		default:
			break;
		}
	}
	else
	{
		switch (channel)
		{
		case 0:
			GPIOSetValue(PUMPS_PORT, PUMPS_1_BIT, 1);
			break;
		case 1:
			GPIOSetValue(PUMPS_PORT, PUMPS_2_BIT, 1);
			break;
		case 2:
			GPIOSetValue(PUMPS_PORT, PUMPS_3_BIT, 1);
			break;
		case 3:
			GPIOSetValue(PUMPS_PORT, PUMPS_4_BIT, 1);
			break;
		case 4:
			GPIOSetValue(PUMPS_PORT, PUMPS_5_BIT, 1);
			break;
		case 5:
			GPIOSetValue(PUMPS_PORT, PUMPS_6_BIT, 1);
			break;
		case 6:
			GPIOSetValue(PUMPS_PORT, PUMPS_7_BIT, 1);
			break;
		case 7:
			GPIOSetValue(PUMPS_PORT, PUMPS_8_BIT, 1);
			break;
		default:
			break;
		}
	}
	PumpsSpeed[channel] = value;
	return TRUE;
}

uint16_t PumpsGetSpeed(uint8_t channel)
{
	if (!(channel >= 1 && channel <= 8))
		return FALSE;
	channel -= 1;
	return PumpsSpeed[channel];
}

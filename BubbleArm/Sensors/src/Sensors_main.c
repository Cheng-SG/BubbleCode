/****************************************************************************
 *   $Id:: blinky_main.c 4785 2010-09-03 22:39:27Z nxp21346                        $
 *   Project: LED flashing / ISP test program
 *
 *   Description:
 *     This file contains the main routine for the blinky code sample
 *     which flashes an LED on the LPCXpresso board and also increments an
 *     LED display on the Embedded Artists base board. This project
 *     implements CRP and is useful for testing bootloaders.
 *
 ****************************************************************************
 * Software that is described herein is for illustrative purposes only
 * which provides customers with programming information regarding the
 * products. This software is supplied "AS IS" without any warranties.
 * NXP Semiconductors assumes no responsibility or liability for the
 * use of the software, conveys no license or title under any patent,
 * copyright, or mask work right to the product. NXP Semiconductors
 * reserves the right to make changes in the software without
 * notification. NXP Semiconductors also make no representation or
 * warranty that such application will be suitable for the specified
 * use without further testing or modification.
 ****************************************************************************/

#include "driver_config.h"
#include "target_config.h"

#include "timer32.h"
#include "gpio.h"
#include "i2c.h"
#include "type.h"
#include "UartPacket.h"

/* In order to start the I2CEngine, the all the parameters
 must be set in advance, including I2CWriteLength, I2CReadLength,
 I2CCmd, and the I2cMasterBuffer which contains the stream
 command/data to the I2c slave device.
 (1) If it's a I2C write only, the number of bytes to be written is
 I2CWriteLength, I2CReadLength is zero, the content will be filled
 in the I2CMasterBuffer.
 (2) If it's a I2C read only, the number of bytes to be read is
 I2CReadLength, I2CWriteLength is 0, the read value will be filled
 in the I2CMasterBuffer.
 (3) If it's a I2C Write/Read with repeated start, specify the
 I2CWriteLength, fill the content of bytes to be written in
 I2CMasterBuffer, specify the I2CReadLength, after the repeated
 start and the device address with RD bit set, the content of the
 reading will be filled in I2CMasterBuffer index at
 I2CMasterBuffer[I2CWriteLength+2].

 e.g. Start, DevAddr(W), WRByte1...WRByteN, Repeated-Start, DevAddr(R),
 RDByte1...RDByteN Stop. The content of the reading will be filled
 after (I2CWriteLength + two devaddr) bytes. */
extern volatile uint8_t I2CMasterBuffer[I2C_BUFSIZE];
extern volatile uint8_t I2CSlaveBuffer[I2C_BUFSIZE];
extern volatile uint32_t I2CMasterState;
extern volatile uint32_t I2CReadLength, I2CWriteLength;

/*Global Variables*/
volatile uint32_t temperature[16][2];
volatile uint8_t TxBuffer[32];
volatile uint32_t TxFlag;

void I2CArbitrationRecovery();

/* Main Program */
int main(void)
{
	uint32_t i = 0, j = 0, n = 0;
	uint16_t tmp;
	/* Basic chip initialization is taken care of in SystemInit() called
	 * from the startup code. SystemInit() and chip settings are defined
	 * in the CMSIS system_<part family>.c file.
	 */

	/* Initialize 32-bit timer 0. TIME_INTERVAL is defined as 10mS */
	/* You may also want to use the Cortex SysTick timer to do this */
	init_timer32(0, SYS_TICK);
	NVIC_EnableIRQ(TIMER_32_0_IRQn);
	/* Enable timer 0. Our interrupt handler will begin incrementing
	 * the TimeTick global each time timer 0 matches and resets.
	 */
	enable_timer32(0);

	/* Initialize GPIO (sets up clock) */
	GPIOInit();
	/* Set LED port pin to output */
	GPIOSetDir(LED_PORT, LED_GREEN_BIT, 1);
	GPIOSetDir(LED_PORT, LED_RED_BIT, 1);
	GPIOSetValue(LED_PORT, LED_GREEN_BIT, 0);
	GPIOSetValue(LED_PORT, LED_RED_BIT, 0);

	//Initialize UART
	PacketInit(115200);

	GPIOSetDir(3, 0, 1);
	GPIOSetDir(3, 1, 1);
	GPIOSetDir(3, 2, 1);
	GPIOSetValue(3, 0, 0);
	GPIOSetValue(3, 1, 0);
	GPIOSetValue(3, 2, 0);

	if (I2CInit((uint32_t) I2CMASTER) == FALSE) /* initialize I2c */
	{
		GPIOSetValue(LED_PORT, LED_RED_BIT, 1);
		while (1)
			; /* Fatal error */
	}

	for (i = 0; i < 16; i++)
	{
		temperature[i][0] = 0;
		temperature[i][1] = 0;
	}
	TxFlag = 0;
	i = 0;

	while (1) /* Loop forever */
	{
		GPIOSetValue(3, 0, (i >> 2) & 0x01);
		GPIOSetValue(3, 1, (i >> 3) & 0x01);
		GPIOSetValue(3, 2, 0);
		/* Write SLA(W), address, SLA(R), and read one byte back. */

		I2CWriteLength = 2;
		I2CReadLength = 2;
		I2CMasterBuffer[0] = (ADT7410_ADDR + (i & 0x03)) << 1;
		I2CMasterBuffer[1] = 0x00; /* address */
		I2CMasterBuffer[2] = ((ADT7410_ADDR + (i & 0x03)) << 1) | RD_BIT;
		for (j = 0; j < 5; j++)
		{
			I2CEngine();
			if (I2CMasterState == I2C_ARBITRATION_LOST)
				I2CArbitrationRecovery();
			else if (I2CMasterState == I2C_OK || I2CMasterState
					== I2C_NACK_ON_ADDRESS)
				break;
		}
		GPIOSetValue(3, 2, 1);
		if (I2CMasterState == I2C_OK)
		{
			tmp = ((((uint16_t) (I2CSlaveBuffer[0])) << 8)
					+ ((uint16_t) (I2CSlaveBuffer[1])));
			tmp >>= 3;
			temperature[i][0] += tmp;
			temperature[i][1]++;
		}
		i++;
		if (i == 16)
		{
			if (TxFlag == 0)
			{
				for (n = 0; n < 16; n++)
				{
					if (temperature[n][1] == 0)
						tmp = 0x4000;
					else
						tmp = temperature[n][0] / temperature[n][1];
					temperature[n][0] = 0;
					temperature[n][1] = 0;
					TxBuffer[(n * 2)] = tmp;
					TxBuffer[(n * 2 + 1)] = (tmp >> 8);
				}
				TxFlag = 1;
			}
		}
		i &= 15;
	}
}

void I2CArbitrationRecovery()
{
	int i, j;
	LPC_IOCON->PIO0_4 &= ~0x3F; /*  I2C I/O configured for IO */
	LPC_IOCON->PIO0_5 &= ~0x3F;
	NVIC_DisableIRQ(I2C_IRQn);
	for (i = 0; i < 32; i++)
	{
		LPC_GPIO0->DATA = (LPC_GPIO0->DATA) ^ (1 << (4));
		for (j = 0; j < 512; j++)
			;
	}
	LPC_IOCON->PIO0_4 &= ~0x3F; /*  I2C I/O config */
	LPC_IOCON->PIO0_4 |= 0x01; /* I2C SCL */
	LPC_IOCON->PIO0_5 &= ~0x3F;
	LPC_IOCON->PIO0_5 |= 0x01; /* I2C SDA */
	NVIC_ClearPendingIRQ(I2C_IRQn);
	NVIC_EnableIRQ(I2C_IRQn);
}

#ifndef CONFIG_TIMER32_DEFAULT_TIMER32_0_IRQHANDLER
volatile uint32_t timer0_count = 0;
volatile uint32_t DataSend = 0;
void TIMER32_0_IRQHandler(void)
{
	if (LPC_TMR32B0->IR & 0x01)
	{
		LPC_TMR32B0->IR = 1; /* clear interrupt flag */
		timer0_count++;
	}
	if (LPC_TMR32B0->IR & (0x1 << 4))
	{
		LPC_TMR32B0->IR = 0x1 << 4; /* clear interrupt flag */
	}

	if (timer0_count % 8 == 0)
		DataSend = 0;

	if (timer0_count % 4 == 0)
		LPC_GPIO0->DATA = (LPC_GPIO0->DATA) ^ (1 << (LED_GREEN_BIT));

	if (timer0_count % 8 == 0 && TxFlag == 1)
	{
		PacketSend((uint8_t*) TxBuffer, 32);
		TxFlag = 0;
	}
	return;
}
#endif

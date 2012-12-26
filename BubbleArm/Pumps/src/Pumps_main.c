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
#include "Pumps.h"
#include "Flowrate.h"

/*Global Variables*/
extern volatile uint32_t Flowrate[8];
volatile uint16_t NewSpeed[8];
volatile uint8_t TxBuffer[TX_BUFSIZE];
volatile uint8_t *RxBuffer;
volatile uint8_t TxFlag;
volatile uint8_t RxCount;
volatile uint16_t TxSum, RxSum;

/* Main Program */
int main(void)
{
	uint32_t n;
	/* Basic chip initialization is taken care of in SystemInit() called
	 * from the startup code. SystemInit() and chip settings are defined
	 * in the CMSIS system_<part family>.c file.
	 */

	/* Initialize 32-bit timer 0. TIME_INTERVAL is defined as 10mS */
	/* You may also want to use the Cortex SysTick timer to do this */
	init_timer32(0, SYS_TICK);
	NVIC_EnableIRQ(TIMER_32_0_IRQn);
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

	FlowrateInit();
	PumpsInit();
	TxFlag = 0;

	while (1) /* Loop forever */
	{
		if (TxFlag == 0)
		{
			for (n = 0; n < 8; n++)
			{
				TxBuffer[(n * 2)] = Flowrate[n];
				TxBuffer[(n * 2 + 1)] = (Flowrate[n] >> 8);
			}
			TxFlag = 1;
		}

		if (PacketReceive(&RxBuffer, &RxCount)==PACKET_RECEIVE_SUCCESS)
		{
			if (RxCount == 16)
			{
				for (n = 0; n < 8; n++)
				{
					NewSpeed[n] = *(RxBuffer+2 * n);
					NewSpeed[n] += (((uint16_t) (*(RxBuffer+2* n + 1))) << 8);
				}
			}
			PumpsSetSpeed(1, NewSpeed[0]);
			PumpsSetSpeed(2, NewSpeed[1]);
			PumpsSetSpeed(3, NewSpeed[2]);
			PumpsSetSpeed(4, NewSpeed[3]);
			PumpsSetSpeed(5, NewSpeed[4]);
			PumpsSetSpeed(6, NewSpeed[5]);
			PumpsSetSpeed(7, NewSpeed[6]);
			PumpsSetSpeed(8, NewSpeed[7]);
		}
	}
}

volatile uint32_t timer0_count = 0;
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

	if (timer0_count % 4 == 0)
		LPC_GPIO0->DATA = (LPC_GPIO0->DATA) ^ (1 << (LED_GREEN_BIT));

	if (timer0_count % 8 == 0 && TxFlag == 1)
	{
		PacketSend(TxBuffer, 16);
		TxFlag = 0;
		LPC_GPIO0->DATA = (LPC_GPIO0->DATA) ^ (1 << (LED_RED_BIT));
	}
	return;
}

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
#include "Pumps.h"
#include "Flowrate.h"
#include "Control.h"
#include "UartPacket.h"

/*Global Variables*/
extern volatile uint32_t Flowrates[8];
volatile uint32_t SetFlowrates[4];
volatile uint32_t NewControlLoop;
uint8_t TxBuffer[16];
uint8_t TxFlap;

/* Main Program */
int main(void)
{
	uint8_t* RecBuff = 0;
	uint8_t RecLength,i;
	/* Basic chip initialization is taken care of in SystemInit() called
	 * from the startup code. SystemInit() and chip settings are defined
	 * in the CMSIS system_<part family>.c file.
	 */
	NewControlLoop = 0;
	TxFlap = 0;

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

	FlowratesInit();
	PumpsInit();
	ControlInit();

	while (1) /* Loop forever */
	{
		if (PacketReceive(RecBuff, &RecLength) == PACKET_RECEIVE_SUCCESS)
		{
			if (RecLength == 8)
			{
				SetFlowrates[0] = (((uint32_t) *(RecBuff + 0)) << 0)
						+ (((uint32_t) *(RecBuff + 1)) << 8);
				SetFlowrates[1] = (((uint32_t) *(RecBuff + 2)) << 0)
						+ (((uint32_t) *(RecBuff + 3)) << 8);
				SetFlowrates[2] = (((uint32_t) *(RecBuff + 4)) << 0)
						+ (((uint32_t) *(RecBuff + 5)) << 8);
				SetFlowrates[3] = (((uint32_t) *(RecBuff + 6)) << 0)
						+ (((uint32_t) *(RecBuff + 7)) << 8);
				SetSpeeds(SetFlowrates);
			}
			LPC_GPIO0->DATA = (LPC_GPIO0->DATA) ^ (1 << (LED_GREEN_BIT));
		}
		if(TxFlap == 0)
		{
			for(i=0;i<8;i++)
			{
				TxBuffer[2*i] = Flowrates[i];
				TxBuffer[2*i+1] = (Flowrates[i]>>8);
			}
			TxFlap = 1;
		}
		if (NewControlLoop == 1)
		{
			Control(Flowrates);
			NewControlLoop = 0;
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

	//	if (timer0_count % 4 == 0)
	//		LPC_GPIO0->DATA = (LPC_GPIO0->DATA) ^ (1 << (LED_GREEN_BIT));

	if (timer0_count % 8 == 0 && TxFlap == 1)
	{
		PacketSend((uint8_t*) Flowrates, 16);
		TxFlap = 0;
		LPC_GPIO0->DATA = (LPC_GPIO0->DATA) ^ (1 << (LED_RED_BIT));
	}

	if (timer0_count % 8 == 4)
	{
		NewControlLoop = 1;
	}
	return;
}

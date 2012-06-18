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
#include "uart.h"
#include "Pumps.h"
#include "Flowrate.h"

/*Global Variables*/
extern volatile uint32_t Flowrate[8];
volatile uint16_t NewSpeed[8];
volatile uint32_t NewSpeedData;
volatile uint8_t  TxBuffer[TX_BUFSIZE];
volatile uint8_t  RxBuffer[RX_BUFSIZE];
volatile uint32_t TxCount;
volatile uint32_t RxCount;
volatile uint16_t TxSum,RxSum;

/* Main Program */
int main(void) {
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
	UARTInit(115200);
	TxCount = TX_BUFSIZE+1;
	RxCount = 0;
	LPC_UART->IER = IER_RBR | IER_THRE;	/* Enable UART interrupt */

	FlowrateInit();
	PumpsInit();
	NewSpeedData = 0;

	while (1) /* Loop forever */
	{
		if ( TxCount == (TX_BUFSIZE+1) )
		{
			TxBuffer[0] = 0xAA;
			TxBuffer[1] = 0x55;
			TxSum = 0x55AA;
			for (n = 0; n < 8; n++) {
				TxBuffer[((n + 1) * 2)] = Flowrate[n];
				TxBuffer[((n + 1) * 2 + 1)] = (Flowrate[n] >> 8);
				TxSum += Flowrate[n];
			}
			TxSum = (~TxSum)+1;
			TxBuffer[((n + 1) * 2)] = TxSum;
			TxBuffer[((n + 1) * 2 + 1)] = (TxSum >> 8);
			TxCount++;
		}
		if(NewSpeedData == 1)
		{
			PumpsSetSpeed(1,NewSpeed[0]);
			PumpsSetSpeed(2,NewSpeed[1]);
			PumpsSetSpeed(3,NewSpeed[2]);
			PumpsSetSpeed(4,NewSpeed[3]);
			PumpsSetSpeed(5,NewSpeed[4]);
			PumpsSetSpeed(6,NewSpeed[5]);
			PumpsSetSpeed(7,NewSpeed[6]);
			PumpsSetSpeed(8,NewSpeed[7]);
			NewSpeedData = 0;
		}
	}
}

volatile uint32_t timer0_count=0;
void TIMER32_0_IRQHandler(void) {
	if (LPC_TMR32B0->IR & 0x01) {
		LPC_TMR32B0->IR = 1; /* clear interrupt flag */
		timer0_count++;
	}
	if (LPC_TMR32B0->IR & (0x1 << 4)) {
		LPC_TMR32B0->IR = 0x1 << 4; /* clear interrupt flag */
	}

	if( timer0_count%4 == 0 )
		LPC_GPIO0->DATA = (LPC_GPIO0->DATA) ^ (1 << (LED_GREEN_BIT));

	if(timer0_count%8 == 0 && TxCount == (TX_BUFSIZE+2) )
	{
		LPC_UART->THR = TxBuffer[0];
		TxCount = 1;
		LPC_GPIO0->DATA = (LPC_GPIO0->DATA) ^ (1 << (LED_RED_BIT));
	}
	return;
}

void UART_IRQHandler(void) {
	uint8_t IIRValue, LSRValue;
	uint8_t temp = temp;
	uint16_t tmp,n;

	IIRValue = LPC_UART->IIR;

	IIRValue >>= 1; /* skip pending bit in IIR */
	IIRValue &= 0x07; /* check bit 1~3, interrupt identification */
	if (IIRValue == IIR_RLS) /* Receive Line Status */
	{
		LSRValue = LPC_UART->LSR;
		/* Receive Line Status */
		if (LSRValue & (LSR_OE | LSR_PE | LSR_FE | LSR_RXFE | LSR_BI)) {
			/* There are errors or break interrupt */
			/* Read LSR will clear the interrupt */
			temp = LPC_UART->RBR; /* temp read on RX to clear
			 interrupt, then bail out */
			return;
		}
		if (LSRValue & LSR_RDR) /* Receive Data Ready */
		{
			/* If no error on RLS, normal ready, save into the data buffer. */
			/* Note: read RBR will clear the interrupt */
			temp = LPC_UART->RBR;
		}
	}
	else if (IIRValue == IIR_RDA) /* Receive Data Available */
	{
		/* Receive Data Available */
		temp = LPC_UART->RBR;

		if (RxCount == 0)
		{
			if (temp == 0xAA)
			{
				RxBuffer[RxCount] = temp;
				RxCount++;
			}
		}
		else if (RxCount == 1)
		{
			if (temp == 0x55)
			{
				RxBuffer[RxCount] = temp;
				RxCount++;
				RxSum = 0x55AA;
			}
			else
			{
				RxCount = 0;
			}
		}
		else if (RxCount < RX_BUFSIZE)
		{
			RxBuffer[RxCount] = temp;
			RxCount++;

			if (RxCount == RX_BUFSIZE)
			{
				for (n = 2; n < (RX_BUFSIZE-2); n++)
				{
					if ((n & 1) == 0)
					{
						RxSum += (uint16_t)RxBuffer[n];
					}
					else
					{
						RxSum += (((uint16_t)RxBuffer[n]) << 8);
					}
				}
			    tmp = (uint16_t)RxBuffer[RX_BUFSIZE-2];
				tmp += (((uint16_t)RxBuffer[RX_BUFSIZE-1]) << 8);
				if (tmp == RxSum)
				{
					RxCount = 0;

					for (n = 0; n < 8; n++)
					{
						NewSpeed[n] = (uint16_t)((uint16_t)(((uint16_t)RxBuffer[2 * n + 3]) << 8) + (uint16_t)RxBuffer[2 * n + 2]);
					}
					NewSpeedData = 1;
				}
			}
		}
		else
		{
			RxCount = 0;
			if (temp == 0xAA)
			{
				RxBuffer[RxCount] = temp;
				RxCount++;
			}
		}
	}
	else if (IIRValue == IIR_CTI) /* Character timeout indicator */
	{
		/* Character Time-out indicator */
	}
	else if (IIRValue == IIR_THRE) /* THRE, transmit holding register empty */
	{
		/* THRE interrupt */
		if (TxCount < TX_BUFSIZE) {
			LPC_UART->THR = TxBuffer[TxCount];
			TxCount++;
		}
		else if(TxCount == TX_BUFSIZE)TxCount++;
	}
	return;
}

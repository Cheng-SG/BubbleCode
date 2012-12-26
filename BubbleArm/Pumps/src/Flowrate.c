/*
 * FlowRate.c
 *
 *  Created on: Apr 2, 2012
 *      Author: vivid
 */

#include "driver_config.h"
#include "target_config.h"

#include "type.h"
#include "timer32.h"
#include "gpio.h"
#include "Flowrate.h"

volatile uint32_t Flowrate[8];
volatile uint32_t count[8];

void FlowrateInit()
{
	uint32_t i;

	/* Initialize GPIO (sets up clock) */
	GPIOInit();
	/* Set flow-rate port pins to input */
	GPIOSetDir(FLOWRATE_PORT, FLOWRATE_1_BIT, 0);
	GPIOSetDir(FLOWRATE_PORT, FLOWRATE_2_BIT, 0);
	GPIOSetDir(FLOWRATE_PORT, FLOWRATE_3_BIT, 0);
	GPIOSetDir(FLOWRATE_PORT, FLOWRATE_4_BIT, 0);
	GPIOSetDir(FLOWRATE_PORT, FLOWRATE_5_BIT, 0);
	GPIOSetDir(FLOWRATE_PORT, FLOWRATE_6_BIT, 0);
	GPIOSetDir(FLOWRATE_PORT, FLOWRATE_7_BIT, 0);
	GPIOSetDir(FLOWRATE_PORT, FLOWRATE_8_BIT, 0);

	//enable interrupt on both edge of the flow-rate pins
	GPIOSetInterrupt(FLOWRATE_PORT, FLOWRATE_1_BIT, 0, 1, 0);
	GPIOIntClear(FLOWRATE_PORT, FLOWRATE_1_BIT);
	GPIOIntEnable(FLOWRATE_PORT, FLOWRATE_1_BIT);

	GPIOSetInterrupt(FLOWRATE_PORT, FLOWRATE_2_BIT, 0, 1, 0);
	GPIOIntClear(FLOWRATE_PORT, FLOWRATE_2_BIT);
	GPIOIntEnable(FLOWRATE_PORT, FLOWRATE_2_BIT);

	GPIOSetInterrupt(FLOWRATE_PORT, FLOWRATE_3_BIT, 0, 1, 0);
	GPIOIntClear(FLOWRATE_PORT, FLOWRATE_3_BIT);
	GPIOIntEnable(FLOWRATE_PORT, FLOWRATE_3_BIT);

	GPIOSetInterrupt(FLOWRATE_PORT, FLOWRATE_4_BIT, 0, 1, 0);
	GPIOIntClear(FLOWRATE_PORT, FLOWRATE_4_BIT);
	GPIOIntEnable(FLOWRATE_PORT, FLOWRATE_4_BIT);

	GPIOSetInterrupt(FLOWRATE_PORT, FLOWRATE_5_BIT, 0, 1, 0);
	GPIOIntClear(FLOWRATE_PORT, FLOWRATE_5_BIT);
	GPIOIntEnable(FLOWRATE_PORT, FLOWRATE_5_BIT);

	GPIOSetInterrupt(FLOWRATE_PORT, FLOWRATE_6_BIT, 0, 1, 0);
	GPIOIntClear(FLOWRATE_PORT, FLOWRATE_6_BIT);
	GPIOIntEnable(FLOWRATE_PORT, FLOWRATE_6_BIT);

	GPIOSetInterrupt(FLOWRATE_PORT, FLOWRATE_7_BIT, 0, 1, 0);
	GPIOIntClear(FLOWRATE_PORT, FLOWRATE_7_BIT);
	GPIOIntEnable(FLOWRATE_PORT, FLOWRATE_7_BIT);

	GPIOSetInterrupt(FLOWRATE_PORT, FLOWRATE_8_BIT, 0, 1, 0);
	GPIOIntClear(FLOWRATE_PORT, FLOWRATE_8_BIT);
	GPIOIntEnable(FLOWRATE_PORT, FLOWRATE_8_BIT);

	for (i = 0; i < 8; i++)
	{
		Flowrate[i] = 0;
		count[i] = 0;
	}
	//initialize time1
	init_timer32(1, SystemCoreClock - 1);
	enable_timer32(1);
	//enable interrupt for timer and pins
	NVIC_EnableIRQ(EINT2_IRQn);
	NVIC_EnableIRQ(TIMER_32_1_IRQn);
}

void PIOINT2_IRQHandler(void)
{
	uint32_t regVal;

	regVal = LPC_GPIO2->MIS;
	LPC_GPIO2->IC = regVal;

	if (regVal & (0x1 << FLOWRATE_1_BIT))
		count[0]++;
	if (regVal & (0x1 << FLOWRATE_2_BIT))
		count[1]++;
	if (regVal & (0x1 << FLOWRATE_3_BIT))
		count[2]++;
	if (regVal & (0x1 << FLOWRATE_4_BIT))
		count[3]++;
	if (regVal & (0x1 << FLOWRATE_5_BIT))
		count[4]++;
	if (regVal & (0x1 << FLOWRATE_6_BIT))
		count[5]++;
	if (regVal & (0x1 << FLOWRATE_7_BIT))
		count[6]++;
	if (regVal & (0x1 << FLOWRATE_8_BIT))
		count[7]++;

	return;
}

void TIMER32_1_IRQHandler(void)
{
	uint32_t i;
	if (LPC_TMR32B1->IR & 0x01)
	{
		LPC_TMR32B1->IR = 1; /* clear interrupt flag */
		for (i = 0; i < 8; i++)
		{
			Flowrate[i] = count[i];
			count[i] = 0;
		}
	}
	if (LPC_TMR32B1->IR & (0x1 << 4))
	{
		LPC_TMR32B1->IR = 0x1 << 4; /* clear interrupt flag */
	}
	return;
}

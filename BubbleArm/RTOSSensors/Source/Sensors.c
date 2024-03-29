#include "driver_config.h"
#include "i2c.h"
#include "gpio.h"
#include "UartPacket.h"
#include "type.h"
#include "main.h"
#include "string.h"
#include "FreeRTOS.h"
#include "task.h"

extern volatile uint8_t I2CMasterBuffer[I2C_BUFSIZE];
extern volatile uint8_t I2CSlaveBuffer[I2C_BUFSIZE];
extern volatile uint32_t I2CMasterState;
extern volatile uint32_t I2CReadLength, I2CWriteLength;

/*Global Variables*/
volatile uint32_t temperature[16][2];
volatile uint8_t TxBuffer[32];

void I2CArbitrationRecovery();

void prvDataSendTask(void *pvParameters)
{
	uint8_t n;
	uint16_t tmp;
	portTickType xNextWakeTime;
	pvParameters = pvParameters;

	//Initialize UART
	PacketInit(230400);
	xNextWakeTime = xTaskGetTickCount();
	for (;;)
	{
		vTaskDelayUntil(&xNextWakeTime,
				1000 / DATA_SEND_FREQUENCY / portTICK_RATE_MS);
		//taskENTER_CRITICAL();
		//destination:0
		TxBuffer[0] = 0;
		TxBuffer[1] = 0;
		//type:0x0100-Flowrates
		TxBuffer[2] = 0x00;
		TxBuffer[3] = 0x00;
		for (n = 0; n < 16; n++)
		{
			if (temperature[n][1] == 0)
				tmp = 0x4000;
			else
				tmp = temperature[n][0] / temperature[n][1];
			temperature[n][0] = 0;
			temperature[n][1] = 0;
			TxBuffer[(n * 2 + 4)] = tmp;
			TxBuffer[(n * 2 + 5)] = (tmp >> 8);
		}
		//taskEXIT_CRITICAL();
		PacketSend((uint8_t*) TxBuffer, 36);
		GPIOToggle(LED_PORT, LED_GREEN_BIT);
	}
}

void prvSensorsScanTask(void *pvParameters)
{
	uint32_t i = 0, j = 0;
	uint16_t tmp;
	portTickType xNextWakeTime;
	pvParameters = pvParameters;
	/* Initialize GPIO (sets up clock) */
	GPIOInit();
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
	i = 0;

	xNextWakeTime = xTaskGetTickCount();
	for (;;)
	{
		/* Place this task in the blocked state until it is time to run again.
		 The block time is specified in ticks, the constant used converts ticks
		 to ms.  While in the Blocked state this task will not consume any CPU
		 time. */
		vTaskDelayUntil(&xNextWakeTime,
				1000 / SENSOR_SCAN_FREQUENCY / portTICK_RATE_MS);
		for (i = 0; i < 16; i++)
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
				else if (I2CMasterState == I2C_OK
						|| I2CMasterState == I2C_NACK_ON_ADDRESS)
					break;
			}
			GPIOSetValue(3, 2, 1);
			if (I2CMasterState == I2C_OK)
			{
				tmp = ((((uint16_t) (I2CSlaveBuffer[0])) << 8)
						+ ((uint16_t) (I2CSlaveBuffer[1])));
				tmp >>= 3;
				//taskENTER_CRITICAL();
				temperature[i][0] += tmp;
				temperature[i][1]++;
				//taskEXIT_CRITICAL();
			}
		}
	}
}

void I2CArbitrationRecovery()
{
	int i, j;
	LPC_IOCON ->PIO0_4 &= ~0x3F; /*  I2C I/O configured for IO */
	LPC_IOCON ->PIO0_5 &= ~0x3F;
	NVIC_DisableIRQ(I2C_IRQn);
	for (i = 0; i < 32; i++)
	{
		LPC_GPIO0 ->DATA = (LPC_GPIO0 ->DATA) ^ (1 << (4));
		for (j = 0; j < 512; j++)
			;
	}
	LPC_IOCON ->PIO0_4 &= ~0x3F; /*  I2C I/O config */
	LPC_IOCON ->PIO0_4 |= 0x01; /* I2C SCL */
	LPC_IOCON ->PIO0_5 &= ~0x3F;
	LPC_IOCON ->PIO0_5 |= 0x01; /* I2C SDA */
	NVIC_ClearPendingIRQ(I2C_IRQn);
	NVIC_EnableIRQ(I2C_IRQn);
}

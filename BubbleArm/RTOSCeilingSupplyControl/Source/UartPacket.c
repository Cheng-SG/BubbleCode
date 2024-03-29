#include "driver_config.h"
#include "uart.h"
#include "Pumps.h"
#include "UartPacket.h"
#include "LPC11xx.h"
#include "FreeRTOS.h"
#include "semphr.h"
#include "Control.h"
#include "main.h"
#include "gpio.h"

uint8_t InputPayload[4][MAX_PAYLOAD_LENGTH];
uint8_t InBuff, InCount, InLength, InTempLength, InSum, NewInData;
uint8_t OutputBuff[MAX_PAYLOAD_LENGTH + 4];
uint8_t OutCount, OutLength, OutBusy;

xSemaphoreHandle xSerialSemaphore = 0;

void prvDataReceiveTask(void *pvParameters)
{
	uint8_t m;
	uint16_t src, type;
	pvParameters = pvParameters;
	vSemaphoreCreateBinary( xSerialSemaphore);
	if (xSerialSemaphore == 0)
	{
		while (1)
			;
	}
	xSemaphoreTake( xSerialSemaphore, portMAX_DELAY);
	for (;;)
	{
		if (xSemaphoreTake( xSerialSemaphore, portMAX_DELAY ) == pdTRUE)
		{
			m = (InBuff+3)&0x03;
			src = InputPayload[m][0];
			src += ((uint16_t) (InputPayload[m][1])) << 8;
			type = InputPayload[m][2];
			type += ((uint16_t) (InputPayload[m][3])) << 8;
			if (src == 0 && type == 0x0101)
			{
				SetSpeeds( (uint16_t*)(&InputPayload[m][4]) );
			}
			if (src == 1 && type == 0x0000)
			{
				SetTemperatures((uint16_t*)(&InputPayload[m][4]));
			}
			GPIOToggle(LED_PORT, LED_RED_BIT);
		}
	}
}

void PacketInit(uint32_t Baudrate)
{
	UARTInit(Baudrate);
	InBuff = 0;
	InCount = 0;
	InLength = 0;
	NewInData = 0;
	LPC_UART ->IER = IER_RBR | IER_THRE; /* Enable UART interrupt */
}

uint8_t PacketReceive(uint8_t* payload, uint8_t *length)
{
	if (NewInData == 1)
	{
		payload = &(InputPayload[(InBuff + 3) & 0x03][0]);
		*length = InLength;
		NewInData = 0;
		InLength = 0;
		return PACKET_RECEIVE_SUCCESS;
	}
	return PACKET_RECEIVE_FAIL;
}

uint8_t PacketSend(uint8_t* payload, uint8_t length)
{
	uint8_t sum, i, tmp;
	if (OutBusy != 0)
		return PACKET_SEND_ERROR_BUSY;
	if (length > MAX_PAYLOAD_LENGTH)
		return PACKET_SEND_ERROR_LENGTH;
	OutputBuff[0] = 0xAA;
	sum = 0xAA;
	OutputBuff[1] = 0x55;
	sum += 0x55;
	OutputBuff[2] = length + 4;
	sum += length + 4;
	for (i = 4; i < length + 4; i++)
	{
		tmp = *(payload + i - 4);
		OutputBuff[i] = tmp;
		sum += tmp;
	}
	OutputBuff[3] = -sum;
	OutLength = length + 4;

	LPC_UART ->THR = OutputBuff[0];
	OutCount = 1;
	OutBusy = 1;
	return PACKET_SEND_SUCCESS;
}

void UART_IRQHandler(void)
{
	uint8_t IIRValue, LSRValue;
	uint8_t temp = temp;
	static signed portBASE_TYPE xHigherPriorityTaskWoken;

	IIRValue = LPC_UART ->IIR;

	IIRValue >>= 1; /* skip pending bit in IIR */
	IIRValue &= 0x07; /* check bit 1~3, interrupt identification */
	if (IIRValue == IIR_RLS) /* Receive Line Status */
	{
		LSRValue = LPC_UART ->LSR;
		/* Receive Line Status */
		if (LSRValue & (LSR_OE | LSR_PE | LSR_FE | LSR_RXFE | LSR_BI))
		{
			/* There are errors or break interrupt */
			/* Read LSR will clear the interrupt */
			temp = LPC_UART ->RBR; /* temp read on RX to clear
			 interrupt, then bail out */
			return;
		}
		if (LSRValue & LSR_RDR) /* Receive Data Ready */
		{
			/* If no error on RLS, normal ready, save into the data buffer. */
			/* Note: read RBR will clear the interrupt */
			temp = LPC_UART ->RBR;
		}
	}
	else if (IIRValue == IIR_RDA) /* Receive Data Available */
	{
		/* Receive Data Available */
		temp = LPC_UART ->RBR;

		if (InCount == 0)
		{
			if (temp == 0xAA)
			{
				InSum = 0xAA;
				InCount++;
			}
		}
		else if (InCount == 1)
		{
			if (temp == 0x55)
			{
				InCount++;
				InSum += 0x55;
			}
			else if (temp != 0xAA)
			{
				InCount = 0;
			}
		}
		else if (InCount == 2)
		{
			InTempLength = temp;
			if (InTempLength > MAX_PAYLOAD_LENGTH + 4)
				InCount = 0;
			else
			{
				InCount++;
				InSum += temp;
			}
		}
		else if (InCount == 3)
		{
			InCount++;
			InSum += temp;
		}
		else if (InCount < MAX_PAYLOAD_LENGTH + 4)
		{
			InputPayload[InBuff][InCount - 4] = temp;
			InCount++;
			InSum += temp;

			if (InCount >= InTempLength)
			{
				if (InSum == 0)
				{
					InBuff = (InBuff + 1) & 0x3;
					InLength = InTempLength - 4;
					NewInData = 1;
					xHigherPriorityTaskWoken = pdFALSE;
					if (xSerialSemaphore != 0)
						xSemaphoreGiveFromISR(xSerialSemaphore,
								&xHigherPriorityTaskWoken);
				}
				InCount = 0;
				InTempLength = 0;
			}
		}
		else
		{
			InCount = 0;
		}
	}
	else if (IIRValue == IIR_CTI) /* Character timeout indicator */
	{
		/* Character Time-out indicator */
	}
	else if (IIRValue == IIR_THRE) /* THRE, transmit holding register empty */
	{
		/* THRE interrupt */
		if (OutCount < OutLength)
		{
			LPC_UART ->THR = OutputBuff[OutCount];
			OutCount++;
		}
		else
		{
			OutCount = 0;
			OutLength = 0;
			OutBusy = 0;
		}
	}
	return;
}

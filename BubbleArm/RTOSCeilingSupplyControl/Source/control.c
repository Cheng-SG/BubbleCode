#include "control.h"
#include "Pumps.h"
#include "type.h"
#include "FreeRTOS.h"
#include "semphr.h"
#include "task.h"
#include "main.h"

extern uint32_t Flowrate[8];
uint16_t Temperature[16];
xSemaphoreHandle xControlSemaphore;
uint16_t SetFlow[2],SetTemp[2];

void ControlInit(void);
void Control(uint32_t* flowrates);

void prvControlTask(void *pvParameters)
{
	pvParameters = pvParameters;
	xControlSemaphore = 0;
	ControlInit();
	vSemaphoreCreateBinary( xControlSemaphore);
	if (xControlSemaphore == 0)
	{
		while (1)
			;
	}

	for (;;)
	{
		xSemaphoreTake(xControlSemaphore, portMAX_DELAY);
		Control(Flowrate);
	}
}

int abs(int value)
{
	if (value < 0)
		return -value;
	return value;
}

uint16_t PIDCalcu(PIDParam* param, int actual_position)
{
	int error;
	int derivative;
	int output;

	//Calculate P,I,D
	error = (param->setpoint) - actual_position;

	//In case of error too small then stop intergration
	if (abs(error) > (param->epsilon))
		(param->integral) = (param->integral) + error;
	derivative = (error - (param->pre_error));
	output = param->output + (param->kp) * error
			+ (param->ki) * param->integral + (param->kd) * derivative;
	//Saturation Filter
	if (output > (param->max))
		output = (param->max);
	else if (output < (param->min))
		output = 1;
	//Update error
	(param->pre_error) = error;
	param->output = output;

	return (uint16_t) output;
}

PIDParam CeilingPumps[4];

void ControlInit()
{
	int i;
	for (i = 0; i < 4; i++)
	{
		CeilingPumps[i].max = MAX;
		CeilingPumps[i].min = MIN;
		CeilingPumps[i].epsilon = EPSILON;
		CeilingPumps[i].pre_error = 0;
		CeilingPumps[i].integral = 0;
		CeilingPumps[i].kp = KP;
		CeilingPumps[i].ki = KI;
		CeilingPumps[i].kd = KD;
		CeilingPumps[i].output = 0;
	}
	SetFlow[0] = 0;
	SetFlow[1] = 0;
	SetTemp[0] = 0;
	SetTemp[0] = 0;
}

void SetSpeeds(uint16_t* values)
{
	SetFlow[0] = *(values);
	SetTemp[0] = *(values + 1);
	SetFlow[1] = *(values + 2);
	SetTemp[1] = *(values + 3);
}

void CalcuSpeeds()
{
	int flow,temp,f1, f2;
	if(Temperature[PANEL1_SUPPLY_TEMPERATURE_CHANNEL - 1] !=0 && Temperature[PANEL1_RECYCLE_TEMPERATURE_CHANNEL - 1] !=0 )
	{
		flow = SetFlow[0];
		temp = SetTemp[0];
		if(temp<Temperature[PANEL1_SUPPLY_TEMPERATURE_CHANNEL - 1])
		{
			CeilingPumps[0].setpoint = flow;
			CeilingPumps[1].setpoint = 0;
		}
		else if(temp>=Temperature[PANEL1_SUPPLY_TEMPERATURE_CHANNEL - 1] && temp < Temperature[PANEL1_RECYCLE_TEMPERATURE_CHANNEL - 1])
		{
			f1 = flow * (temp - Temperature[PANEL1_SUPPLY_TEMPERATURE_CHANNEL - 1])
				/ (Temperature[PANEL1_SUPPLY_TEMPERATURE_CHANNEL - 1]
						- Temperature[PANEL1_RECYCLE_TEMPERATURE_CHANNEL - 1]);
			f2 = flow - f1;
			CeilingPumps[0].setpoint = f1;
			CeilingPumps[1].setpoint = f2;
		}
		else
		{
			CeilingPumps[0].setpoint = 0;
			CeilingPumps[1].setpoint = flow;
		}
	}
	else
	{
		CeilingPumps[0].setpoint = 0;
		CeilingPumps[1].setpoint = 0;
	}

	if(Temperature[PANEL2_SUPPLY_TEMPERATURE_CHANNEL - 1] !=0 && Temperature[PANEL2_RECYCLE_TEMPERATURE_CHANNEL - 1] !=0 )
	{
		flow = SetFlow[1];
		temp = SetTemp[1];
		if(temp<Temperature[PANEL2_SUPPLY_TEMPERATURE_CHANNEL - 1])
		{
			CeilingPumps[2].setpoint = flow;
			CeilingPumps[3].setpoint = 0;
		}
		else if(temp>=Temperature[PANEL2_SUPPLY_TEMPERATURE_CHANNEL - 1] && temp < Temperature[PANEL2_RECYCLE_TEMPERATURE_CHANNEL - 1])
		{
			f1 = flow * (temp - Temperature[PANEL2_SUPPLY_TEMPERATURE_CHANNEL - 1])
				/ (Temperature[PANEL2_SUPPLY_TEMPERATURE_CHANNEL - 1]
						- Temperature[PANEL2_RECYCLE_TEMPERATURE_CHANNEL - 1]);
			f2 = flow - f1;
			CeilingPumps[2].setpoint = f1;
			CeilingPumps[3].setpoint = f2;
		}
		else
		{
			CeilingPumps[2].setpoint = 0;
			CeilingPumps[3].setpoint = flow;
		}
	}
	else
	{
		CeilingPumps[2].setpoint = 0;
		CeilingPumps[3].setpoint = 0;
	}
}

void SetTemperatures(uint16_t* values)
{
	uint32_t i;
	for (i = 0; i < 16; i++)
		Temperature[i] = *(values + i);
}

void Control(uint32_t* flowrates)
{
	uint16_t out;
	CalcuSpeeds();
	out = PIDCalcu(&(CeilingPumps[0]),
			*(flowrates + (PANEL1_SUPPLY_FLOWRATE_CHANNEL - 1)));
	PumpsSetSpeed(PANEL1_SUPPLY_PUMP_CHANNEL, out );
	out = PIDCalcu(&(CeilingPumps[1]),
			*(flowrates + (PANEL1_RECYCLE_FLOWRATE_CHANNEL - 1)));
	PumpsSetSpeed(PANEL1_RECYCLE_FLOWRATE_CHANNEL, out);
	out = PIDCalcu(&(CeilingPumps[2]),
			*(flowrates + (PANEL2_SUPPLY_FLOWRATE_CHANNEL - 1)));
	PumpsSetSpeed(PANEL2_SUPPLY_PUMP_CHANNEL, out );
	out = PIDCalcu(&(CeilingPumps[3]),
			*(flowrates + (PANEL2_RECYCLE_FLOWRATE_CHANNEL - 1)));
	PumpsSetSpeed(PANEL2_RECYCLE_FLOWRATE_CHANNEL, out);
}


#include "Control.h"
#include "Pumps.h"
#include "type.h"
#include "FreeRTOS.h"
#include "semphr.h"
#include "task.h"

extern uint32_t Flowrate[8];
xSemaphoreHandle xControlSemaphore;

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

void Pump1PIDControl(PIDParam* param, int actual_position)
{
	int error;
	int derivative;
	int output;

	if (param->setpoint == 0)
	{
		PumpsSetSpeed(1, 0);
		PumpsSetSpeed(2, 0);
	}
	else if (param->setpoint < param->setpointMin)
	{
		PumpsSetSpeed(1, 1);
		PumpsSetSpeed(2, 0);
	}
	else if (param->setpoint < param->setpointMid)
	{
		//Calculate P,I,D
		error = (param->setpoint) - actual_position;

		//In case of error too small then stop intergration
		if (abs(error) > (param->epsilon))
			(param->integral) = (param->integral) + error;
		derivative = (error - (param->pre_error));
		output = param->output + (param->kp[0]) * error
				+ (param->ki) * param->integral + (param->kd) * derivative;
		//Saturation Filter
		if (output > (param->max))
		{
			output = (param->max);
		}
		else if (output < (param->min))
		{
			output = 1;
		}
		//Update error
		(param->pre_error) = error;
		param->output = output;

		PumpsSetSpeed(1, output);
		PumpsSetSpeed(2, 0);
	}
	else if (param->setpoint < param->setpointMax)
	{
		//Calculate P,I,D
		error = (param->setpoint) - actual_position;

		//In case of error too small then stop intergration
		if (abs(error) > (param->epsilon))
			(param->integral) = (param->integral) + error;
		derivative = (error - (param->pre_error));
		output = param->output + (param->kp[1]) * error
				+ (param->ki) * param->integral + (param->kd) * derivative;
		//Saturation Filter
		if (output > (param->max))
			output = (param->max);
		else if (output < (param->min))
			output = 1;
		//Update error
		(param->pre_error) = error;
		param->output = output;

		PumpsSetSpeed(1, output);
		PumpsSetSpeed(2, output);
	}
	else
	{
		PumpsSetSpeed(1, 65535);
		PumpsSetSpeed(2, 65535);
	}
}

void Pump2PIDControl(PIDParam* param, int actual_position)
{
	int error;
	int derivative;
	int output;

	if (param->setpoint == 0)
	{
		PumpsSetSpeed(3, 0);
		PumpsSetSpeed(4, 0);
	}
	else if (param->setpoint < param->setpointMin)
	{
		PumpsSetSpeed(3, 1);
		PumpsSetSpeed(4, 0);
	}
	else if (param->setpoint < param->setpointMid)
	{
		//Calculate P,I,D
		error = (param->setpoint) - actual_position;

		//In case of error too small then stop intergration
		if (abs(error) > (param->epsilon))
			(param->integral) = (param->integral) + error;
		derivative = (error - (param->pre_error));
		output = param->output + (param->kp[0]) * error
				+ (param->ki) * param->integral + (param->kd) * derivative;
		//Saturation Filter
		if (output > (param->max))
			output = (param->max);
		else if (output < (param->min))
			output = 1;
		//Update error
		(param->pre_error) = error;
		param->output = output;

		PumpsSetSpeed(3, output);
		PumpsSetSpeed(4, 0);
	}
	else if (param->setpoint < param->setpointMax)
	{
		//Calculate P,I,D
		error = (param->setpoint) - actual_position;

		//In case of error too small then stop intergration
		if (abs(error) > (param->epsilon))
			(param->integral) = (param->integral) + error;
		derivative = (error - (param->pre_error));
		output = param->output + (param->kp[1]) * error
				+ (param->ki) * param->integral + (param->kd) * derivative;
		//Saturation Filter
		if (output > (param->max))
			output = (param->max);
		else if (output < (param->min))
			output = 1;
		//Update error
		(param->pre_error) = error;
		param->output = output;

		PumpsSetSpeed(3, output);
		PumpsSetSpeed(4, output);
	}
	else
	{
		PumpsSetSpeed(3, 65535);
		PumpsSetSpeed(4, 65535);
	}
}

void Pump3PIDControl(PIDParam* param, int actual_position)
{
	int error;
	int derivative;
	int output;

	if (param->setpoint == 0)
	{
		PumpsSetSpeed(5, 0);
	}
	else if (param->setpoint < param->setpointMin)
	{
		PumpsSetSpeed(5, 1);
	}
	else if (param->setpoint < param->setpointMax)
	{
		//Calculate P,I,D
		error = (param->setpoint) - actual_position;

		//In case of error too small then stop intergration
		if (abs(error) > (param->epsilon))
			(param->integral) = (param->integral) + error;
		derivative = (error - (param->pre_error));
		output = param->output + (param->kp[0]) * error
				+ (param->ki) * param->integral + (param->kd) * derivative;
		//Saturation Filter
		if (output > (param->max))
			output = (param->max);
		else if (output < (param->min))
			output = 1;
		//Update error
		(param->pre_error) = error;
		param->output = output;

		PumpsSetSpeed(5, output);
	}
	else
	{
		PumpsSetSpeed(5, 65535);
	}

}

void Pump4PIDControl(PIDParam* param, int actual_position)
{
	int error;
	int derivative;
	int output;

	if (param->setpoint == 0)
	{
		PumpsSetSpeed(6, 0);
	}
	else if (param->setpoint < param->setpointMin)
	{
		PumpsSetSpeed(6, 1);
	}
	else if (param->setpoint < param->setpointMax)
	{
		//Calculate P,I,D
		error = (param->setpoint) - actual_position;

		//In case of error too small then stop intergration
		if (abs(error) > (param->epsilon))
			(param->integral) = (param->integral) + error;
		derivative = (error - (param->pre_error));
		output = param->output + (param->kp[0]) * error
				+ (param->ki) * param->integral + (param->kd) * derivative;
		//Saturation Filter
		if (output > (param->max))
			output = (param->max);
		else if (output < (param->min))
			output = 1;
		//Update error
		(param->pre_error) = error;
		param->output = output;

		PumpsSetSpeed(6, output);
	}
	else
	{
		PumpsSetSpeed(6, 65535);
	}
}

PIDParam Airbox1, Airbox2, Airbox3, Airbox4;

void ControlInit()
{
	int temp;
	Airbox1.max = MAX;
	Airbox1.min = MIN;
	Airbox1.epsilon = EPSILON;
	Airbox1.pre_error = 0;
	Airbox1.integral = 0;
	Airbox1.ki = AB1KI;
	Airbox1.kd = AB1KD;
	Airbox1.output = 0;
	PumpsSetSpeed(1, 1);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox1.setpointMin = Flowrate[0];
	PumpsSetSpeed(1, 65535);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox1.kp[0] = 65535.0 / (Flowrate[0] - Airbox1.setpointMin);
	PumpsSetSpeed(1, 1);
	PumpsSetSpeed(2, 1);
	vTaskDelay(10 * configTICK_RATE_HZ );
	temp = Flowrate[0];
	PumpsSetSpeed(1, 65535);
	PumpsSetSpeed(2, 65535);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox1.setpointMax = Flowrate[0];
	Airbox1.kp[1] = 65535.0 / (Airbox1.setpointMax - temp);
	PumpsSetSpeed(1, 6553);
	PumpsSetSpeed(2, 6553);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox1.setpointMid = Flowrate[0];
	PumpsSetSpeed(1, 0);
	PumpsSetSpeed(2, 0);
	vTaskDelay(5 * configTICK_RATE_HZ );
	Airbox1.kp[0] /= 50;
	Airbox1.kp[1] /= 50;

	Airbox2.max = MAX;
	Airbox2.min = MIN;
	Airbox2.epsilon = EPSILON;
	Airbox2.pre_error = 0;
	Airbox2.integral = 0;
	Airbox2.ki = AB2KI;
	Airbox2.kd = AB2KD;
	Airbox2.output = 0;
	PumpsSetSpeed(3, 1);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox2.setpointMin = Flowrate[1];
	PumpsSetSpeed(3, 65535);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox2.kp[0] = 65535.0 / (Flowrate[1] - Airbox2.setpointMin);
	PumpsSetSpeed(3, 1);
	PumpsSetSpeed(4, 1);
	vTaskDelay(10 * configTICK_RATE_HZ );
	temp = Flowrate[1];
	PumpsSetSpeed(3, 65535);
	PumpsSetSpeed(4, 65535);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox2.setpointMax = Flowrate[1];
	Airbox2.kp[1] = 65535.0 / (Airbox2.setpointMax - temp);
	PumpsSetSpeed(3, 6553);
	PumpsSetSpeed(4, 6553);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox2.setpointMid = Flowrate[1];
	PumpsSetSpeed(3, 0);
	PumpsSetSpeed(4, 0);
	vTaskDelay(5 * configTICK_RATE_HZ );
	Airbox2.kp[0] /= 50;
	Airbox2.kp[1] /= 50;

	Airbox3.max = MAX;
	Airbox3.min = MIN;
	Airbox3.epsilon = EPSILON;
	Airbox3.pre_error = 0;
	Airbox3.integral = 0;
	Airbox3.ki = AB3KI;
	Airbox3.kd = AB3KD;
	Airbox3.output = 0;
	PumpsSetSpeed(5, 1);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox3.setpointMin = Flowrate[2];
	PumpsSetSpeed(5, 65535);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox3.setpointMax = Flowrate[2];
	Airbox3.kp[0] = 65535.0 / (Airbox3.setpointMax - Airbox3.setpointMin);
	PumpsSetSpeed(5, 0);
	vTaskDelay(5 * configTICK_RATE_HZ );
	Airbox3.kp[0] /= 30;

	Airbox4.max = MAX;
	Airbox4.min = MIN;
	Airbox4.epsilon = EPSILON;
	Airbox4.pre_error = 0;
	Airbox4.integral = 0;
	Airbox4.ki = AB4KI;
	Airbox4.kd = AB4KD;
	Airbox4.output = 0;
	PumpsSetSpeed(6, 1);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox4.setpointMin = Flowrate[3];
	PumpsSetSpeed(6, 65535);
	vTaskDelay(10 * configTICK_RATE_HZ );
	Airbox4.setpointMax = Flowrate[3];
	Airbox4.kp[0] = 65535.0 / (Airbox4.setpointMax - Airbox4.setpointMin);
	PumpsSetSpeed(6, 0);
	Airbox4.kp[0] /= 30;
}

void SetSpeeds(uint32_t* values)
{
	Airbox1.setpoint = *values;
	Airbox2.setpoint = *(values + 1);
	Airbox3.setpoint = *(values + 2);
	Airbox4.setpoint = *(values + 3);
}

void Control(uint32_t* flowrates)
{
	Pump1PIDControl(&(Airbox1), *(flowrates + AB1_SENSOR_CH));
	Pump2PIDControl(&(Airbox2), *(flowrates + AB2_SENSOR_CH));
	Pump3PIDControl(&(Airbox3), *(flowrates + AB3_SENSOR_CH));
	Pump4PIDControl(&(Airbox4), *(flowrates + AB4_SENSOR_CH));
}


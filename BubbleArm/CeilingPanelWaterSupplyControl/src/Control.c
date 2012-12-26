#include "Control.h"
#include "Pumps.h"
#include "type.h"

int abs(int value)
{
	if(value<0)
		return -value;
	return value;
}

int PIDCalcu(PIDParam* param,int actual_position)
{
	int error;
	int derivative;
	int output;

	//Calculate P,I,D
	error = (param->setpoint) - actual_position;

	//In case of error too small then stop intergration
	if(abs(error) > (param->epsilon) )
	{
		(param->integral) = (param->integral) + error*(param->dt);
	}
	derivative = (error - (param->pre_error))/(param->dt);
	output = (param->kp)*error + (param->ki)*param->integral + (param->kd)*derivative;

	//Saturation Filter
	if(output > (param->max) )
	{
		output = (param->max);
	}
	else if(output < (param->min))
	{
		output = (param->min);
	}
	//Update error
	(param->pre_error) = error;

	return output;
}

PIDParam Airbox1,Airbox2,Airbox3,Airbox4;

void ControlInit()
{
	Airbox1.max = MAX;
	Airbox1.min = MIN;
	Airbox1.epsilon = EPSILON;
	Airbox1.dt = DT;
	Airbox1.pre_error = 0;
	Airbox1.integral = 0;
	Airbox1.kp = AB1KP;
	Airbox1.ki = AB1KI;
	Airbox1.kd = AB1KD;

	Airbox2.max = MAX;
	Airbox2.min = MIN;
	Airbox2.epsilon = EPSILON;
	Airbox2.dt = DT;
	Airbox2.pre_error = 0;
	Airbox2.integral = 0;
	Airbox2.kp = AB2KP;
	Airbox2.ki = AB2KI;
	Airbox2.kd = AB2KD;

	Airbox3.max = MAX;
	Airbox3.min = MIN;
	Airbox3.epsilon = EPSILON;
	Airbox3.dt = DT;
	Airbox3.pre_error = 0;
	Airbox3.integral = 0;
	Airbox3.kp = AB3KP;
	Airbox3.ki = AB3KI;
	Airbox3.kd = AB3KD;

	Airbox4.max = MAX;
	Airbox4.min = MIN;
	Airbox4.epsilon = EPSILON;
	Airbox4.dt = DT;
	Airbox4.pre_error = 0;
	Airbox4.integral = 0;
	Airbox4.kp = AB4KP;
	Airbox4.ki = AB4KI;
	Airbox4.kd = AB4KD;
}

void SetSpeeds(uint32_t* values)
{
		Airbox1.setpoint = *values;
		Airbox2.setpoint = *(values+1);
		Airbox3.setpoint = *(values+2);
		Airbox4.setpoint = *(values+3);
}

void Control(uint32_t* flowrates)
{
	int output[4];
	output[0] = PIDCalcu(&(Airbox1),*(flowrates+AB1_SENSOR_CH));
	output[0] = 65536 - output[0];
	output[1] = PIDCalcu(&(Airbox2),*(flowrates+AB2_SENSOR_CH));
	output[1] = 65536 - output[1];
	output[2] = PIDCalcu(&(Airbox3),*(flowrates+AB3_SENSOR_CH));
	output[2] = 65536 - output[2];
	output[3] = PIDCalcu(&(Airbox4),*(flowrates+AB4_SENSOR_CH));
	output[3] = 65536 - output[3];

	PumpsSetSpeed(1, output[0]);
	PumpsSetSpeed(2, output[0]);
	PumpsSetSpeed(3, output[1]);
	PumpsSetSpeed(4, output[1]);
	PumpsSetSpeed(5, output[2]);
	PumpsSetSpeed(6, output[3]);
}



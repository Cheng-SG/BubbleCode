#ifndef _CONTROL_H_
#define _CONTROL_H_

#include "type.h"

typedef struct
{
	int   epsilon;
	int   max;
	int   min;
	float kp;
	float ki;
	float kd;
	int   pre_error;
	float integral;
	int   setpoint;
	int   setpointMax;
	int   setpointMid;
	int   setpointMin;
	int   output;
} PIDParam;

void ControlInit(void);
void SetSpeeds(uint16_t* values);
void SetTemperatures(uint16_t* values);
void Control(uint32_t* flowrates);
void prvControlTask(void *pvParameters);

#endif

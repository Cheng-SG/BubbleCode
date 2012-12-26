#ifndef PID_H_
#define PID_H_

#include "type.h"

typedef struct
{
	int   epsilon;
	int   max;
	int   min;
	float kp[2];
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

#define MAX     65535
#define MIN     0
#define EPSILON 2

#define AB1KI   0
#define AB1KD   0
#define AB1_SENSOR_CH 0

#define AB2KI   0
#define AB2KD   0
#define AB2_SENSOR_CH 1

#define AB3KI   0
#define AB3KD   0
#define AB3_SENSOR_CH 2

#define AB4KI   0
#define AB4KD   0
#define AB4_SENSOR_CH 3

void ControlInit(void);
void SetSpeeds(uint32_t* values);
void Control(uint32_t* flowrates);
void prvControlTask(void *pvParameters);

#endif

#ifndef PID_H_
#define PID_H_

#include "type.h"

typedef struct
{
	int epsilon;
	int dt;
	int max;
	int min;
	int kp;
	int ki;
	int kd;
	int pre_error;
	int integral;
	int setpoint;
} PIDParam;

#define MAX     65536
#define MIN     (65536-62259)
#define DT      1
#define EPSILON 4

#define AB1KP   1
#define AB1KI   1
#define AB1KD   0
#define AB1_SENSOR_CH 0

#define AB2KP   1
#define AB2KI   1
#define AB2KD   0
#define AB2_SENSOR_CH 1

#define AB3KP   1
#define AB3KI   1
#define AB3KD   0
#define AB3_SENSOR_CH 2

#define AB4KP   1
#define AB4KI   1
#define AB4KD   0
#define AB4_SENSOR_CH 3

void ControlInit(void);
void SetSpeeds(uint32_t* values);
void Control(uint32_t* flowrates);

#endif

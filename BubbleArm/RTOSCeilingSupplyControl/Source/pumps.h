/*
 * Pumps.h
 *
 *  Created on: Apr 2, 2012
 *      Author: vivid
 */

#ifndef _PUMPS_H_
#define _PUMPS_H_

#define PUMPS_PORT  1
#define PUMPS_1_BIT 0
#define PUMPS_2_BIT 1
#define PUMPS_3_BIT 4
#define PUMPS_4_BIT 5
#define PUMPS_5_BIT 8
#define PUMPS_6_BIT 9
#define PUMPS_7_BIT 10
#define PUMPS_8_BIT 11

#define AD5669R_ADDR 0x57


uint32_t PumpsInit();
uint32_t PumpsSetSpeed(uint8_t channel, uint16_t value);
uint16_t PumpsGetSpeed(uint8_t channel);

#endif /* PUMPS_H_ */

#ifndef UART_PACKET_H
#define UART_PACKET_H

#include "type.h"

#define PACKET_SEND_SUCCESS       0
#define PACKET_SEND_ERROR_BUSY    1
#define PACKET_SEND_ERROR_LENGTH  2
#define PACKET_RECEIVE_SUCCESS    0
#define PACKET_RECEIVE_FAIL       1

#define MAX_PAYLOAD_LENGTH 64

void PacketInit(uint32_t Baudrate);
uint8_t PacketReceive(uint8_t* payload,uint8_t *length);
uint8_t PacketSend(uint8_t* payload,uint8_t length);

#endif

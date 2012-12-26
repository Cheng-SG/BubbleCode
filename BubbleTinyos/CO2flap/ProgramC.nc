#include "Timer.h"
#include "I2C.h"
#include "msp430usart.h"

module ProgramC
{
    uses interface Timer<TMilli> as Timer0;
    uses interface Leds;
    uses interface Boot;

    uses interface CO2flap;

    uses interface SplitControl as RadioControl;
    uses interface Packet;
    uses interface AMPacket;
    uses interface AMSend;
    uses interface PacketAcknowledgements as Ack;
    uses interface Receive;
}
implementation
{
#define BASESTATION_ID 0
#define MAX_RETRY      5

    uint8_t   cnt,RetryCount;
    message_t pkt;

    task void SendTask();

    event void Boot.booted()
    {
        call CO2flap.init();
        call RadioControl.start();
    }  

    event void RadioControl.startDone(error_t error)
    {
        if(error != SUCCESS)
        {
            call RadioControl.start();
        }
        else 
        {
            call Timer0.startPeriodic(512);
        }
    }

    event void RadioControl.stopDone(error_t error)
    {
    }

    event void Timer0.fired()
    {
        uint8_t* data;
        cnt++;
        call Leds.led1Toggle();
        if(cnt&1)
        {
            if(call CO2flap.isOnline()==TRUE)
            {
                data = (uint8_t*)(call Packet.getPayload(&pkt,12));
                *data++ = 0x00;
                *data++ = 0x03;
                call CO2flap.read(0,data);
                data++;
                call CO2flap.read(1,data);
                data++;
                call CO2flap.read(10,data);
                data++;
                call CO2flap.read(11,data);
                data++;
                call CO2flap.read(12,data);
                data++;
                call CO2flap.read(13,data);
                data++;
                call CO2flap.read(14,data);
                data++;
                call CO2flap.read(30,data);
                data++;
                call CO2flap.read(31,data);
                data++;
                call CO2flap.read(32,data);
                RetryCount=0;
                post SendTask();
            }
        }
    }

    task void SendTask()
    {
        if(RetryCount < MAX_RETRY)
        {
            call Ack.requestAck(&pkt);
            if( call AMSend.send(BASESTATION_ID,&pkt,12) != SUCCESS)
            {
                RetryCount++;
                post SendTask();
            }
        }
        else
        {
            RetryCount = 0;
            call Leds.led0Toggle();
        }
    }

    event void AMSend.sendDone(message_t* msg, error_t error)
    {
        if(call Ack.wasAcked(msg))
        {
            call Leds.led2Toggle();
        }
        else
        {
            RetryCount++;
            post SendTask();
        }
    }

    event message_t* Receive.receive(message_t* msg, void* payload, uint8_t len)
    {
        uint8_t type,length;
        uint8_t address,data;
        uint8_t i,*pdata;

        if(call AMPacket.isForMe(msg)==TRUE)
        {
            pdata = (uint8_t*)payload;
            length = *pdata;
            type = *(pdata+1);
            if(type == 0x03)
            {
                for(i=0;i<length;i++)
                {
                    address = *(pdata+2+i*2);
                    data    = *(pdata+3+i*2);
                    call CO2flap.write(address, data);
                }
            }
            call Leds.led0Toggle();
        }
        return msg;
    }
}


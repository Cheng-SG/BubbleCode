#include "Timer.h"
#include "I2C.h"
#include "msp430usart.h"

module ProgramC
{
    uses interface Timer<TMilli> as Timer0;
    uses interface Leds;
    uses interface Boot;

    uses interface Airbox;

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
    bool      Rbusy;
    message_t pkt;

    task void SendTask();

    event void Boot.booted()
    {
        call Airbox.init();
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
            if(call Airbox.isOnline()==TRUE)
            {
                data = (uint8_t*)(call Packet.getPayload(&pkt,8));
                *data++ = 0x00;
                *data++ = 0x02;
                call Airbox.read(0,data);
                data++;
                call Airbox.read(1,data);
                data++;
                call Airbox.read(14,data);
                data++;
                call Airbox.read(32,data);
                data++;
                call Airbox.read(33,data);
                data++;
                call Airbox.read(34,data);
                RetryCount = 0;
                post SendTask();
                if(SUCCESS == call AMSend.send(0, &pkt, 8))
                {
                    Rbusy = TRUE;
                    call Leds.led2Toggle();
                }
            }
        }
    }

    task void SendTask()
    {
        if(RetryCount < MAX_RETRY)
        {
            call Ack.requestAck(&pkt);
            if( call AMSend.send(BASESTATION_ID,&pkt,8) != SUCCESS)
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
            type = *pdata;
            length = *(pdata+1);
            if(type == 0x02)
            {
                for(i=0;i<length;i++)
                {
                    address = *(pdata+2+i*2);
                    data    = *(pdata+3+i*2);
                    call Airbox.write(address, data);
                }
            }
            call Leds.led0Toggle();
        }
        return msg;
    }

}


#include "Timer.h"
#include "I2C.h"
#include "msp430usart.h"

module ProgramC
{
    uses interface Timer<TMilli> as Timer0;
    uses interface Leds;
    uses interface Boot;

    uses interface MySerial;

    uses interface SplitControl as RadioControl;
    uses interface Packet;
    uses interface AMPacket;
    uses interface AMSend;
    uses interface PacketAcknowledgements as Ack;
    uses interface Receive;
    uses interface Receive as Snoop;
}
implementation
{
#define BASESTATION_ID 0
#define MAX_RETRY      5

    norace uint8_t buf[36];
    norace uint8_t *bp;
    message_t pkt;
    norace bool    Rbusy;
    uint8_t RetryCount;

    task void SendTask();

    event void Boot.booted()
    {
        Rbusy = FALSE;
        call MySerial.init();
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
            call Timer0.startPeriodic(4);
            WDTCTL = 0x5A09;
        }
    }

    event void RadioControl.stopDone(error_t error)
    {
    }

    event void Timer0.fired()
    {
        //call Leds.led1Toggle();
        WDTCTL = 0x5A09;
    }

    event message_t* Receive.receive(message_t* msg, void* payload, uint8_t len)
    {
        uint8_t  *p;
        uint8_t  i;
        uint16_t src;
        if(TOS_NODE_ID == BASESTATION_ID)
            return msg;
        else 
        {
            src = call AMPacket.source(msg);
            p = (uint8_t*)payload;
            buf[0] = src;
            buf[1] = (src>>8);
            for(i=0;i<len;i++)
            {
                buf[i+2]=*(p+i);
            }
            if(call MySerial.send(buf,len+2) == SUCCESS)
                call Leds.led1Toggle();
            else
                call Leds.led0Toggle();
        }
        call Leds.led1Toggle();
        return msg;
    }
    
    event message_t* Snoop.receive(message_t* msg, void* payload, uint8_t len)
    {
        uint8_t  *p;
        uint8_t  i;
        uint16_t src;
        if(TOS_NODE_ID == BASESTATION_ID)
            return msg;
        else 
        { 
            src = call AMPacket.source(msg);
            p = (uint8_t*)payload;
            buf[0] = src;
            buf[1] = (src >> 8);
            for(i=0;i<len;i++)
            {
                buf[i+2]=*(p+i);
            }
            if(call MySerial.send(buf,len+2) == SUCCESS)
                call Leds.led1Toggle();
            else
                call Leds.led0Toggle();
        }
        return msg;
    }
     

    event void MySerial.sendDone(){}

    event void MySerial.receive(uint8_t *payload,uint8_t len)
    {
        uint8_t* data;
        uint16_t dst;
        uint8_t  i;
        if( Rbusy == FALSE )
        {
            if(TOS_NODE_ID == 0)
                return;
            else
            {
                dst = *payload;
                dst += ((uint16_t)(*(payload+1)))<<8;
                data = (uint8_t*)(call Packet.getPayload(&pkt,18));
                for(i=2;i<len;i++)*data++ = *(payload+i);
                call AMPacket.setDestination(&pkt,dst);
                RetryCount = 0;
                Rbusy = TRUE;
                post SendTask();
            }
        }
    }

    task void SendTask()
    {
        uint16_t dst;
        if(RetryCount < MAX_RETRY)
        {
            call Ack.requestAck(&pkt);
            dst = call AMPacket.destination(&pkt);
            if( call AMSend.send(dst,&pkt,18) != SUCCESS)
            {
                RetryCount++;
                post SendTask();
            }				
        }
        else
        {
            RetryCount = 0;
            Rbusy = FALSE;
            call Leds.led0Toggle();
        }
    }

    event void AMSend.sendDone(message_t* msg, error_t error)
    {
        if(call Ack.wasAcked(msg))
        {
            RetryCount = 0;
            call Leds.led2Toggle();
            Rbusy = FALSE;
        }
        else
        {
            RetryCount++;
            post SendTask();
        }
    }
}


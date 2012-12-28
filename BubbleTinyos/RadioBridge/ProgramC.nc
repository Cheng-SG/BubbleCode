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
    enum
    {
        UARTBUF_SIZE = 43,
        UARTQUEUE_SIZE = 32,
        RADIOQUEUE_SIZE = 32,
        MAX_RETRY = 5,
    };

    uint8_t Ubuf[UARTQUEUE_SIZE][UARTBUF_SIZE];
    uint8_t Uin,Uout;
    bool    Ufull,Ubusy;

    message_t Rbuf[RADIOQUEUE_SIZE];
    uint8_t Rlen[RADIOQUEUE_SIZE];
    uint8_t Rin,Rout,RetryCount;
    bool    Rfull,Rbusy;

    event void Boot.booted()
    {
        Uin=0;Uout=0;
        Ufull= FALSE;
        Ubusy = FALSE;
        
        Rin=0;Rout=0;RetryCount=0;
        Rfull= FALSE;
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

    task void Uart_sendTask()
    {
        atomic
        {
            if(Uin == Uout && Ufull == FALSE)
            {
                Ubusy = FALSE;
                return;
            }
        }
        if(call MySerial.send(Ubuf[Uout],Ubuf[Uout][UARTBUF_SIZE-1]) != SUCCESS)
            post Uart_sendTask();
    }

    event message_t* Receive.receive(message_t* msg, void* payload, uint8_t len)
    {
        uint8_t* p;
        uint8_t  i;
        uint16_t src;
        if(len>UARTBUF_SIZE-1)
        {
            call Leds.led0Toggle();
            return msg;
        }
        src = call AMPacket.source(msg);
        if(Ufull == FALSE)
        {
            Ubuf[Uin][0] = src;
            Ubuf[Uin][1] = (src>>8);
            p = (uint8_t*)payload;
            for(i=0;i<len;i++)
            {
                Ubuf[Uin][i+2]=*p++;
            }
            Ubuf[Uin][UARTBUF_SIZE-1]=len+2;

            atomic
            {
                Uin = (Uin+1) % UARTQUEUE_SIZE;
                if(Uin == Uout)
                    Ufull = TRUE;
            }
            if(Ubusy == FALSE)
            {
                post Uart_sendTask();
                Ubusy = TRUE; 
            }
        }
        else
        {
            call Leds.led0Toggle();
        }
        return msg;
    }
    
    event message_t* Snoop.receive(message_t* msg, void* payload, uint8_t len)
    {
        uint8_t* p;
        uint8_t  i;
        uint16_t src;
        if(len>UARTBUF_SIZE-1)
        {
            call Leds.led0Toggle();
            return msg;
        }
        src = call AMPacket.source(msg);
        if(Ufull == FALSE)
        {
            Ubuf[Uin][0] = src;
            Ubuf[Uin][1] = (src>>8);
            p = (uint8_t*)payload;
            for(i=0;i<len;i++)
            {
                Ubuf[Uin][i+2]=*p++;
            }
            Ubuf[Uin][UARTBUF_SIZE-1]=len+2;

            atomic
            {
                Uin = (Uin+1) % UARTQUEUE_SIZE;
                if(Uin == Uout)
                    Ufull = TRUE;
            }
            if(Ubusy == FALSE)
            {
                post Uart_sendTask();
                Ubusy = TRUE; 
            }
        }
        else
        {
            call Leds.led0Toggle();
        }
        return msg;
    }

    event void MySerial.sendDone()
    {
        atomic
        {
            Uout = (Uout+1) % UARTQUEUE_SIZE;
            Ufull = FALSE;
        }
        post Uart_sendTask();
        call Leds.led2Toggle();
    }
    
    task void Radio_sendTask()
    {
        uint16_t dst;
        atomic
        {
            if(Rin == Rout && Rfull == FALSE)
            {
                Rbusy = FALSE;
                return;
            }
        }

        dst = call AMPacket.destination(&(Rbuf[Rout]));
        if(dst == TOS_NODE_ID)
        {
            atomic
            {
                Rout = (Rout+1) % RADIOQUEUE_SIZE;
                if(Rfull == TRUE)Rfull = FALSE;
                RetryCount = 0;
            }
            post Radio_sendTask();
            return;
        }
        call Ack.requestAck(&(Rbuf[Rout]));
        if(call AMSend.send(dst,&(Rbuf[Rout]),Rlen[Rout])!=SUCCESS)
        {
            atomic
            {
                RetryCount++;
                if(RetryCount >= MAX_RETRY)
                {
                    Rout = (Rout+1) % RADIOQUEUE_SIZE;
                    if(Rfull == TRUE)Rfull = FALSE;
                    RetryCount = 0;
                    call Leds.led0Toggle();
                }
            }
            post Radio_sendTask();
        }
    }

    event void AMSend.sendDone(message_t* msg, error_t error)
    {
        atomic
        {
            if(call Ack.wasAcked(msg))
            {
                Rout = (Rout+1) % RADIOQUEUE_SIZE;
                if(Rfull == TRUE)Rfull = FALSE;
                RetryCount = 0;
                call Leds.led1Toggle();
            }
            else
            {
                RetryCount++;
                if(RetryCount >= MAX_RETRY)
                {
                    Rout = (Rout+1) % RADIOQUEUE_SIZE;
                    if(Rfull == TRUE)Rfull = FALSE;
                    RetryCount = 0;
                    call Leds.led0Toggle();
                }
            }
            
        }
        post Radio_sendTask();
    }

    event void MySerial.receive(uint8_t *rbuf,uint8_t len)
    {
        uint8_t* data;
        uint8_t  i;
        uint16_t dst;
        if(Rfull == FALSE)
        {
            data = (uint8_t*)(call Packet.getPayload(&(Rbuf[Rin]),len-2));
            dst = *rbuf++;
            dst += (((uint16_t)*rbuf)<<8);
            rbuf++;
            for(i=0;i<len-2;i++)*data++ = *rbuf++;
            call AMPacket.setDestination(&(Rbuf[Rin]),dst);
            Rlen[Rin] = len-2;

            atomic
            {
                Rin = (Rin+1) % RADIOQUEUE_SIZE;
                if(Rin == Rout)
                    Rfull = TRUE;
                if(Rbusy == FALSE)
                {
                    post Radio_sendTask();
                    Rbusy = TRUE;
                }
            }
        }
        else
        {
            call Leds.led0Toggle(); 
        }

        //call Leds.led0Toggle(); 
    }
}


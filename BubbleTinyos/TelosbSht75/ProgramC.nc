module ProgramC
{
    uses interface Timer<TMilli> as Timer0;
    uses interface Leds;
    uses interface Boot;

    uses interface Read<uint16_t> as Temperature1;
    uses interface Read<uint16_t> as Humidity1;
    uses interface Read<uint16_t> as Temperature2;
    uses interface Read<uint16_t> as Humidity2;

    uses interface SplitControl as RadioControl;
    uses interface Packet;
    uses interface AMPacket;
    uses interface AMSend;
    uses interface PacketAcknowledgements as Ack;
}
implementation
{
#define BASESTATION_ID 0
#define MAX_RETRY      5

    uint16_t temp1,humi1;
    uint16_t temp2,humi2;
    message_t pkt;
    uint8_t RetryCount;

    task void SendTask();

    event void Boot.booted()
    {
        call Timer0.startPeriodic(2048);
    }

    event void Timer0.fired()
    {
        call Temperature1.read();
        //call Leds.led1Toggle();
    }

    event void Temperature1.readDone(error_t result, uint16_t val)
    {
        if(result == SUCCESS)
        {
            temp1 = val;
            call Humidity1.read();
        }
    }

    event void Humidity1.readDone(error_t result, uint16_t val)
    {
        if(result == SUCCESS)
        {
            humi1 = val;
            call Temperature2.read();
        }
    }

    event void Temperature2.readDone(error_t result, uint16_t val)
    {
        if(result == SUCCESS)
        {
            temp2 = val;
            call Humidity2.read();
        }
    }

    event void Humidity2.readDone(error_t result, uint16_t val)
    {
        if(result == SUCCESS)
        {
            humi2 = val;
            call RadioControl.start();
        }
    }

    event void RadioControl.startDone(error_t error)
    {
        uint8_t* data;
        if(error == SUCCESS)
        {
            data = (uint8_t*)(call Packet.getPayload(&pkt,10));
            *data++ = 0x01;
            *data++ = 0x04;
            *data++ = temp1;
            *data++ = (temp1 >> 8);
            *data++ = humi1;
            *data++ = (humi1 >> 8);
            *data++ = temp2;
            *data++ = (temp2 >> 8);
            *data++ = humi2;
            *data++ = (humi2 >> 8);
            RetryCount = 0;
            post SendTask();
        }
        else
        {
            call RadioControl.stop();
        }
    }

    task void SendTask()
    {
        if(RetryCount < MAX_RETRY)
        {
            call Ack.requestAck(&pkt);
            if( call AMSend.send(BASESTATION_ID,&pkt,10) != SUCCESS)
            {
                RetryCount++;
                post SendTask();
            }
        }
        else
        {
            RetryCount = 0;
            call RadioControl.stop();
            //call Leds.led0Toggle();
        }
    }

    event void AMSend.sendDone(message_t* msg, error_t error)
    {
        if(call Ack.wasAcked(msg))
        {
            RetryCount = 0;
            call RadioControl.stop();
            //call Leds.led1Toggle();
        }
        else
        {
            RetryCount++;
            post SendTask();
        }
    }

    event void RadioControl.stopDone(error_t error)
    {
    }

}


module ProgramC
{
    uses interface Timer<TMilli> as Timer0;
    uses interface Leds;
    uses interface Boot;

    uses interface Read<uint16_t> as Temperature;
    uses interface Read<uint16_t> as Humidity;

    uses interface SplitControl as RadioControl;
    uses interface Packet;
    uses interface AMPacket;
    uses interface AMSend;
}
implementation
{
#define BASESTATION_ID 0

    uint8_t cnt;
    uint8_t ReadCnt;
    uint16_t temp,humi;
    message_t pkt;

    event void Boot.booted()
    {
        cnt = 0;
        ReadCnt = 0;
        call Timer0.startPeriodic(2048);
    }

    event void Timer0.fired()
    {
        call Temperature.read();
        //call Leds.led1Toggle();
    }

    event void Temperature.readDone(error_t result, uint16_t val)
    {
        if(result == SUCCESS)
        {
            temp = val;
            call Humidity.read();
        }
    }

    event void Humidity.readDone(error_t result, uint16_t val)
    {
        if(result == SUCCESS)
        {
            humi = val;
            call RadioControl.start();
        }
    }

    event void RadioControl.startDone(error_t error)
    {
        uint8_t* data;
        if(error == SUCCESS)
        {
            data = (uint8_t*)(call Packet.getPayload(&pkt,6));
            *data++ = 0x00;
            *data++ = 0x04;
            *data++ = temp;
            *data++ = (temp >> 8);
            *data++ = humi;
            *data++ = (humi >> 8);
            if( call AMSend.send(BASESTATION_ID,&pkt,6) != SUCCESS)
            {
                call RadioControl.stop();
            }
        }
        else
            call RadioControl.stop();
    }

    event void AMSend.sendDone(message_t* msg, error_t error)
    {
        call RadioControl.stop();
        //call Leds.led1Toggle();
    }

    event void RadioControl.stopDone(error_t error)
    {
    }

}


module ProgramC
{
    uses interface Timer<TMilli> as Timer0;
    uses interface Leds;
    uses interface Boot;

    uses interface Read<uint16_t> as Temperature;
    uses interface Read<uint16_t> as Humidity;

    uses interface SplitControl as RadioControl;
    uses interface StdControl as RoutingControl;
    uses interface Send as CtpSend;
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
            call RoutingControl.start();
            call Timer0.startPeriodic(2048);
        }
    }

    event void RadioControl.stopDone(error_t error)
    {
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


    void SendPacket();

    event void Humidity.readDone(error_t result, uint16_t val)
    {
        if(result == SUCCESS)
        {
            humi = val;
            SendPacket();
        }
    }

    void SendPacket()
    {
        uint8_t* data;

        data = (uint8_t*)(call CtpSend.getPayload(&pkt,6));
        *data++ = 0x00;
        *data++ = 0x04;
        *data++ = temp;
        *data++ = (temp >> 8);
        *data++ = humi;
        *data++ = (humi >> 8);
        if( call CtpSend.send(&pkt,6) != SUCCESS)
        {
            call Leds.led0On();
        }
    }

    event void CtpSend.sendDone(message_t* msg, error_t error)
    {
        //call Leds.led1Toggle();
    }

}


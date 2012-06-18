module ProgramC
{
    uses interface Timer<TMilli> as Timer0;
    uses interface Leds;
    uses interface Boot;

    uses interface SplitControl as RadioControl;
    uses interface Packet;
    uses interface AMPacket;
    uses interface AMSend;
}
implementation
{
#define BASESTATION_ID 0

    uint16_t cnt_t,cnt_r;
    message_t pkt;

    event void Boot.booted()
    {
        cnt_t = 0;
        cnt_r = 0;
        call RadioControl.start();
    }

    event void Timer0.fired()
    {
        uint8_t* data;
        data = (uint8_t*)(call Packet.getPayload(&pkt,4));
        *data++ = cnt_t;
        *data++ = (cnt_t >> 8);
        *data++ = cnt_r;
        *data++ = (cnt_r >> 8);
        if( call AMSend.send(BASESTATION_ID,&pkt,4) == SUCCESS)
            cnt_r ++;
        cnt_t++;
    }

    event void AMSend.sendDone(message_t* msg, error_t error)
    {
    }

    event void RadioControl.startDone(error_t error)
    {
        if(error != SUCCESS)
            call RadioControl.start();
        else
            call Timer0.startPeriodic(1);
    }

    event void RadioControl.stopDone(error_t error)
    {
    }

}


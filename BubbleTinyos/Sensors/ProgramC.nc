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
}
implementation
{
#define BASESTATION_ID 0
#define MAX_RETRY      5

    norace uint8_t buf[36];
    norace uint8_t *bp;
    norace uint8_t DataBuf[32];
    message_t pkt;
    norace uint8_t TxState;
    uint8_t RetryCount;

    task void SendTask();

    event void Boot.booted()
    {
        TxState = 0;
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

    task void SendTask()
    {
        if(RetryCount < MAX_RETRY)
        {
            call Ack.requestAck(&pkt);
            if( call AMSend.send(BASESTATION_ID,&pkt,18) != SUCCESS)
//            if( call AMSend.send(AM_BROADCAST_ADDR,&pkt,18) != SUCCESS)
            {
                RetryCount++;
                post SendTask();
            }
        }
        else
        {
            RetryCount = 0;
            TxState = 0;
            call Leds.led0Toggle();
        }
    }

    event void AMSend.sendDone(message_t* msg, error_t error)
    {
        uint8_t *data,*rbuf,i;
        if(call Ack.wasAcked(msg))
        {
            TxState++;
            if(TxState == 1)
            {
                data = (uint8_t*)(call Packet.getPayload(&pkt,18));
                *data++ = 0x01;
                *data++ = 0x00;
                rbuf = &(DataBuf[16]);
                for(i=0;i<16;i++)*data++ = *rbuf++;
                RetryCount = 0;
                post SendTask();
            }
            else if(TxState >= 2)
            {
                TxState = 0;
                call Leds.led2Toggle();
            }
        }
        else
        {
            RetryCount++;
            post SendTask();
        }
    }

    event message_t* Receive.receive(message_t* msg, void* payload, uint8_t len)
    {
        return msg;
    }

    event void MySerial.sendDone(){}

    event void MySerial.receive(uint8_t *payload,uint8_t len)
    {
        uint8_t* data;
        uint8_t  i;
        if( TOS_NODE_ID != BASESTATION_ID && len==32 && TxState == 0 )
        {
            data = DataBuf;
            for(i=0;i<32;i++)*data++ = *payload++;

            data = (uint8_t*)(call Packet.getPayload(&pkt,18));
            *data++ = 0x00;
            *data++ = 0x00;
            payload = DataBuf;
            for(i=0;i<16;i++)*data++ = *payload++;
            RetryCount = 0;
            post SendTask();
        }
    }
}


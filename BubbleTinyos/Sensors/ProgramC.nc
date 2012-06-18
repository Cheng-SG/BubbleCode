module ProgramC
{
  uses interface Timer<TMilli> as Timer0;
  uses interface Leds;
  uses interface Boot;

  uses interface PumpSerial;

  uses interface SplitControl as RadioControl;
  uses interface Packet;
  uses interface AMPacket;
  uses interface AMSend;
  uses interface Receive;
}
implementation
{
#define BASESTATION_ID 0
 
  norace uint8_t buf[36];
  norace uint8_t *bp;
  norace uint8_t DataBuf[32];
  message_t pkt;
  norace uint8_t TxState;

  event void Boot.booted()
  {
    TxState = 0;
    call PumpSerial.init();
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
      call Timer0.startPeriodic(500);
    }
  }

  event void RadioControl.stopDone(error_t error)
  {
  }

  event void Timer0.fired()
  {
    call Leds.led1Toggle();
  }

  event void AMSend.sendDone(message_t* msg, error_t error)
  {
      uint8_t* data;
      uint8_t* rbuf;
      uint8_t  i;
      if(TxState == 1)
      {
        data = (uint8_t*)(call Packet.getPayload(&pkt,18));
		*data++ = 0x01;
		*data++ = 0x00;
		rbuf = &(DataBuf[16]);
		for(i=0;i<16;i++)*data++ = *rbuf++;
		if(SUCCESS == call AMSend.send(BASESTATION_ID, &pkt, 18))
		{
		   TxState ++;
		}
		else TxState = 0;
      }
      else TxState = 0;
  }

  event message_t* Receive.receive(message_t* msg, void* payload, uint8_t len)
  {
     return msg;
  }
  
  event void PumpSerial.sendDone(){}

  event void PumpSerial.receive(uint8_t *rbuf)
  {
    uint8_t* data;
    uint8_t  i;
    if( TOS_NODE_ID!=0 && TxState == 0 )
    {
		data = DataBuf;
		rbuf += 2;
		for(i=0;i<32;i++)*data++ = *rbuf++;

		data = (uint8_t*)(call Packet.getPayload(&pkt,18));
		*data++ = 0x00;
		*data++ = 0x00;
		rbuf = DataBuf;
		for(i=0;i<16;i++)*data++ = *rbuf++;
		if(SUCCESS == call AMSend.send(BASESTATION_ID, &pkt, 18) )
		{
			TxState ++;
			call Leds.led2Toggle();
		}
    }
    call Leds.led0Toggle();
  }
}


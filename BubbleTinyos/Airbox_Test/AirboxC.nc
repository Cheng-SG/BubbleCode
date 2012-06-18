configuration AirboxC
{
  provides interface Airbox;
}
implementation
{
  components AirboxP;
  components LedsC;
  components new TimerMilliC() as AirboxTimerC;
  components HplMsp430Usart1C;
  components ActiveMessageC;
  components new AMSenderC(0x80);
  components new AMReceiverC(0x80);
  
  Airbox = AirboxP;
  AirboxP.Leds -> LedsC;
  AirboxP.AirboxTimer -> AirboxTimerC;
  AirboxP.HplUart1 -> HplMsp430Usart1C;
  AirboxP.Uart1Interrupts -> HplMsp430Usart1C;

  AirboxP.RadioControl -> ActiveMessageC;
  AirboxP.Packet -> AMSenderC;
  AirboxP.AMPacket -> AMSenderC;
  AirboxP.AMSend -> AMSenderC;
}

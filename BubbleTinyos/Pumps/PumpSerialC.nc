configuration PumpSerialC
{
  provides interface PumpSerial;
}
implementation
{
  components PumpSerialP;
  components HplMsp430Usart1C;
  
  PumpSerial = PumpSerialP;
  PumpSerialP.HplUart1 -> HplMsp430Usart1C;
  PumpSerialP.Uart1Interrupts -> HplMsp430Usart1C;
}

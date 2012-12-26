configuration MySerialC
{
    provides interface MySerial;
}
implementation
{
    components MySerialP;
    components HplMsp430Usart1C;
    components LedsC;

    MySerial = MySerialP;
    MySerialP.HplUart1 -> HplMsp430Usart1C;
    MySerialP.Uart1Interrupts -> HplMsp430Usart1C;
    MySerialP.Leds -> LedsC;
}

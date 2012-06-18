configuration CO2flapC
{
    provides interface CO2flap;
}
implementation
{
    components CO2flapP;
    components LedsC;
    components new TimerMilliC() as CO2flapTimerC;
    components HplMsp430Usart1C;

    CO2flap = CO2flapP;
    CO2flapP.Leds -> LedsC;
    CO2flapP.CO2flapTimer -> CO2flapTimerC;
    CO2flapP.HplUart1 -> HplMsp430Usart1C;
    CO2flapP.Uart1Interrupts -> HplMsp430Usart1C;
}

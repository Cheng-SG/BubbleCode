configuration ProgramAppC
{
}
implementation
{
    components MainC, ProgramC as AppC, LedsC;
    components new TimerMilliC() as Timer0;
    components new SensirionSht75_1C() as Sht1C;
    components new SensirionSht75_2C() as Sht2C;

    components ActiveMessageC;
    components new AMSenderC(0x80);

    AppC -> MainC.Boot;

    AppC.Timer0 -> Timer0;
    AppC.Leds -> LedsC;

    AppC.Temperature1 -> Sht1C.Temperature;
    AppC.Humidity1 -> Sht1C.Humidity;
    AppC.Temperature2 -> Sht2C.Temperature;
    AppC.Humidity2 -> Sht2C.Humidity;

    AppC.RadioControl -> ActiveMessageC;
    AppC.Ack -> ActiveMessageC;
    AppC.Packet -> AMSenderC;
    AppC.AMPacket -> AMSenderC;
    AppC.AMSend -> AMSenderC;
}


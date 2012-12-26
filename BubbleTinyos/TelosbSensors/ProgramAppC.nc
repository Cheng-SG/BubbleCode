configuration ProgramAppC
{
}
implementation
{
    components MainC, ProgramC as AppC, LedsC;
    components new TimerMilliC() as Timer0;
    components new SensirionSht11C() as Sht11C;

    components ActiveMessageC;
    components new AMSenderC(0x80);

    AppC -> MainC.Boot;

    AppC.Timer0 -> Timer0;
    AppC.Leds -> LedsC;

    AppC.Temperature -> Sht11C.Temperature;
    AppC.Humidity -> Sht11C.Humidity;

    AppC.RadioControl -> ActiveMessageC;
    AppC.Ack -> ActiveMessageC;
    AppC.Packet -> AMSenderC;
    AppC.AMPacket -> AMSenderC;
    AppC.AMSend -> AMSenderC;
}


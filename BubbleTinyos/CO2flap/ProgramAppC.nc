configuration ProgramAppC
{
}
implementation
{
    components MainC, ProgramC as AppC, LedsC;
    components new TimerMilliC() as Timer0;
    components CO2flapC;

    components ActiveMessageC;
    components new AMSenderC(0x80);
    components new AMReceiverC(0x80);

    AppC -> MainC.Boot;

    AppC.Timer0 -> Timer0;
    AppC.Leds -> LedsC;

    AppC.CO2flap -> CO2flapC;

    AppC.RadioControl -> ActiveMessageC;
    AppC.Ack -> ActiveMessageC;
    AppC.Packet -> AMSenderC;
    AppC.AMPacket -> AMSenderC;
    AppC.AMSend -> AMSenderC;
    AppC.Receive -> AMReceiverC;
}


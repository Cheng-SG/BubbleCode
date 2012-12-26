configuration ProgramAppC
{
}
implementation
{
    components MainC, ProgramC as AppC, LedsC;
    components new TimerMilliC() as Timer0;
    components MySerialC;

    components ActiveMessageC;
    components new AMSenderC(0x80);
    components new AMReceiverC(0x80);
    //components new AMSnooperC(0x80);

    AppC -> MainC.Boot;

    AppC.Timer0 -> Timer0;
    AppC.Leds -> LedsC;

    AppC.MySerial -> MySerialC;

    AppC.RadioControl -> ActiveMessageC;
    AppC.Ack -> ActiveMessageC;
    AppC.Packet -> AMSenderC;
    AppC.AMPacket -> AMSenderC;
    AppC.AMSend -> AMSenderC;
    AppC.Receive -> AMReceiverC;
    //AppC.Snoop -> AMSnooperC;
}


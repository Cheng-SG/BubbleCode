configuration HalSensirionSht75_1C 
{
    provides interface Resource[ uint8_t client ];
    provides interface SensirionSht75[ uint8_t client ];
}
implementation 
{
    components new SensirionSht75LogicP();
    SensirionSht75 = SensirionSht75LogicP;

    components HplSensirionSht75_1C;
    Resource = HplSensirionSht75_1C.Resource;
    SensirionSht75LogicP.DATA -> HplSensirionSht75_1C.DATA;
    SensirionSht75LogicP.CLOCK -> HplSensirionSht75_1C.SCK;
    SensirionSht75LogicP.InterruptDATA -> HplSensirionSht75_1C.InterruptDATA;

    components new TimerMilliC();
    SensirionSht75LogicP.Timer -> TimerMilliC;

    components LedsC;
    SensirionSht75LogicP.Leds -> LedsC;
}

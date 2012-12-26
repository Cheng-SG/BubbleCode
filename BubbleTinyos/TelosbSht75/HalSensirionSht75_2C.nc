configuration HalSensirionSht75_2C 
{
    provides interface Resource[ uint8_t client ];
    provides interface SensirionSht75[ uint8_t client ];
}
implementation 
{
    components new SensirionSht75LogicP();
    SensirionSht75 = SensirionSht75LogicP;

    components HplSensirionSht75_2C;
    Resource = HplSensirionSht75_2C.Resource;
    SensirionSht75LogicP.DATA -> HplSensirionSht75_2C.DATA;
    SensirionSht75LogicP.CLOCK -> HplSensirionSht75_2C.SCK;
    SensirionSht75LogicP.InterruptDATA -> HplSensirionSht75_2C.InterruptDATA;

    components new TimerMilliC();
    SensirionSht75LogicP.Timer -> TimerMilliC;

    components LedsC;
    SensirionSht75LogicP.Leds -> LedsC;
}

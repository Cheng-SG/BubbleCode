#include "msp430usart.h"

module CO2flapP
{
    provides interface CO2flap;

    uses interface Leds;  
    uses interface Timer<TMilli> as CO2flapTimer; 
    uses interface HplMsp430Usart as HplUart1;
    uses interface HplMsp430UsartInterrupts as Uart1Interrupts;
}

implementation
{
    norace bool    Online,Online_t;
    norace uint8_t State;
    norace uint8_t Address;
    norace bool    Sbusy;
    norace uint8_t Register[64];

    command void CO2flap.init()
    {
        msp430_uart_union_config_t config =
        {
            {
                utxe : 1,
                urxe : 1,
                ubr : UBR_1MHZ_9600,
                umctl : UMCTL_1MHZ_9600,
                ssel : 0x02,
                pena : 0,
                pev : 0,
                spb : 0,
                clen : 1,
                listen : 0,
                mm : 0,
                ckpl : 0,
                urxse : 0,
                urxeie : 1,
                urxwie : 0,
                utxe : 1,
                urxe : 1
            }
        };
        atomic
        {
            call HplUart1.setModeUart(&config);
            call HplUart1.enableIntr();
        }
        Sbusy = FALSE;
        Online = FALSE;
        Online_t = FALSE;

        Register[1] = 4;
        Register[2] = 0;
        Register[3] = 0;
        Register[10]= 90;
        Register[11]= 30;
        Register[12]= 150;
        Register[13]= 10;
        Register[14]= 0xE0;
        Register[15]= 0;
        Register[16]= 0;
        Register[17]= 100;
        Register[18]= 80;
        Register[19]= 38;
        Register[20]= 0;
        Register[21]= 0;
        Register[22]= 200;
        Register[23]= 24;
        Register[24]= 200;
        Register[25]= 13;
        Register[26]= 0;
        Register[27]= 200;
        Register[28]= 0;
        Register[29]= 1;
        call CO2flapTimer.startPeriodic(1024); 
    }


    async command bool CO2flap.isOnline(void)
    {
        return Online;
    }

    async command bool CO2flap.read(uint8_t address,uint8_t* data)
    {
        if(address>=0 && address<=3)
        {
            *data=Register[address];
            return TRUE;
        }
        else if(address>=10 && address<=41)
        {
            *data=Register[address];
            return TRUE;
        }
        else if(address>=62 && address<=63)
        {
            *data=Register[address];
            return TRUE;
        }
        return FALSE;
    }

    async command bool CO2flap.write(uint8_t address,uint8_t data)
    {
        if(address>=1 && address<=3)
        {
            switch(address)
            {
                case 1:
                    if(data<5)
                    {
                        Register[address] = data;
                        return TRUE;
                    }
                    else return FALSE;
                    break;
                case 2:
                    Register[address] = data & 0x01;
                    return TRUE;
                    break;
                case 3:
                    if(data<3)
                    {
                        Register[address] = data;
                        return TRUE;
                    }
                    else return FALSE;
                    break;
                default:
                    break;
            }
        }
        else if(address>=10 && address<=29)
        {
            switch(address)
            {
                case 10:
                case 11:
                case 12:
                case 13:
                    Register[address] = data;
                    return TRUE;
                    break;
                case 14:
                    Register[address] = data & 0xE0;
                    return TRUE;
                    break;
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                    Register[address] = data;
                    return TRUE;
                    break;
                default:
                    return FALSE;
            }
        }
        return FALSE;
    }


    event void CO2flapTimer.fired()
    {
        if(Online_t == TRUE)Online = TRUE;
        else                Online = FALSE;
        Online_t = FALSE;
    }

    async event void Uart1Interrupts.txDone()
    {
        Sbusy = FALSE;
        Online_t = TRUE;
        call Leds.led2Toggle();
    }

    async event void Uart1Interrupts.rxDone(uint8_t data)
    {
        if(State == 0)
        {
            if(data == 0xFF)State = 1;
        }
        else if(State == 1)
        {
            Address = data&0x3F;
            if( (data&0x80) == 0)
            {
                if(Sbusy == FALSE)
                {
                    Sbusy = TRUE;
                    call HplUart1.tx(Register[Address]);
                }
                State = 0;
            }
            else
            {
                State = 2;
            }
        }
        else if(State == 2)
        {
            if(Sbusy == FALSE)
            {
                Register[Address] = data;
                Sbusy=TRUE;
                call HplUart1.tx(data);
            }
            State = 0;
        }
        else State = 0;
    } 

}

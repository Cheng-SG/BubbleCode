#include "msp430usart.h"

module AirboxP
{
    provides interface Airbox;

    uses interface Leds; 
    uses interface Timer<TMilli> as AirboxTimer; 
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

    command void Airbox.init()
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

        Register[1] = 0;
        Register[2] = 0;
        Register[3] = 0;
        Register[10]= 1;
        Register[11]= 15;
        Register[12]= 200;
        Register[13]= 125;
        Register[14]= 150;
        Register[15]= 175;
        Register[16]= 0x20;
        Register[17]= 0;
        Register[18]= 0;
        Register[19]= 20;
        Register[20]= 157;
        Register[21]= 0;
        Register[22]= 100;
        Register[23]= 170;
        Register[24]= 200;

        call AirboxTimer.startPeriodic(1024);
    }

    async command bool Airbox.isOnline(void)
    {
        return Online;
    }

    async command bool Airbox.read(uint8_t address,uint8_t* data)
    {
        if(address<=3)
        {
            *data=Register[address];
            return TRUE;
        }
        else if(address>=10 && address<=24)
        {
            *data=Register[address];
            return TRUE;
        }
        else if(address>=30 && address<=34)
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

    async command bool Airbox.write(uint8_t address,uint8_t data)
    {
        if(address>=1 && address<=3)
        {
            switch(address)
            {
                case 1:
                    if(data<7)
                    {
                        Register[address] = data;
                        return TRUE;
                    }
                    else return FALSE;
                    break;
                case 2:
                    if(data == 0)
                    {
                        Register[address] = data;
                        return TRUE;
                    }
                    else return FALSE;
                    break;
                case 3:
                    if(data<2)
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
        else if(address>=10 && address<=24)
        {
            switch(address)
            {
                case 10:
                    if(data<2)
                    {
                        Register[address] = data;
                        return TRUE;
                    }
                    else return FALSE;
                    break;
                case 11:
                    if(data<60)
                    {
                        Register[address] = data;
                        return TRUE;
                    }
                    else return FALSE;
                    break;
                case 12:
                case 13:
                case 14:
                case 15:
                    Register[address] = data;
                    return TRUE;
                    break;
                case 16:
                    if(data>4 && data<8)
                    {
                        Register[address] = data;
                        return TRUE;
                    }
                    else return FALSE;
                    break;
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                    Register[address] = data;
                    return TRUE;
                    break;
                case 23:
                case 24:
                    if(data>=100 && data<=200)
                    {
                        Register[address] = data;
                        return TRUE;
                    }
                    else return FALSE;
                default:
                    return FALSE;
            }
        }
        return FALSE;
    }

    event void AirboxTimer.fired()
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

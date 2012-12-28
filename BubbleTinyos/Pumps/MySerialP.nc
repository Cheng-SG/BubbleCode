#include "msp430usart.h"

module MySerialP
{
    provides interface MySerial;

    uses interface HplMsp430Usart as HplUart1;
    uses interface HplMsp430UsartInterrupts as Uart1Interrupts;
    uses interface Leds;
}
implementation
{
    enum
    {
        SBUF_LENGTH = 54,
        RBUF_LENGTH = 54
    };

    norace uint8_t Sbuf[SBUF_LENGTH];
    norace uint8_t Scount,Stotalnum;
    norace uint8_t Ssum;
    norace bool    Sbusy;

    norace uint8_t Rbuf0[RBUF_LENGTH];
    norace uint8_t Rbuf1[RBUF_LENGTH];
    norace uint8_t *prbuf,*pprbuf;;
    norace uint8_t Rcount;
    norace uint8_t Rlen,Rlength;
    norace uint8_t Rsum;

    command void MySerial.init()
    {
        msp430_uart_union_config_t config =
        {
            {
                utxe : 1,
                urxe : 1,
                ubr : UBR_1MHZ_230400,
                umctl : UMCTL_1MHZ_230400,
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
            call HplUart1.enableUart();
            Sbusy = FALSE;
            Rcount = 0;
            prbuf = Rbuf0;
        }
    }

    command error_t MySerial.send(uint8_t *sbuf,uint8_t len)
    {
        if(sbuf == NULL)return FAIL;
        if(len > SBUF_LENGTH-4)return FAIL;
        if(Sbusy == TRUE)return EBUSY;

        Sbuf[0] = 0xAA;
        Ssum    = 0xAA;
        Sbuf[1] = 0x55;
        Ssum   += 0x55;
        Sbuf[2] = len+4;
        Ssum   += len+4;

        for(Scount=4;Scount<len+4;Scount++)
        {
            Sbuf[Scount]=*sbuf;
            Ssum += *sbuf;
            sbuf++;
        }
        Ssum = (~Ssum)+1;
        Sbuf[3] = Ssum;
        Scount=0;
        Stotalnum = len+4;
        atomic Sbusy = TRUE;
        call HplUart1.tx(Sbuf[Scount]);
        return SUCCESS;
    }

    task void sendDoneTask()
    {
        atomic Sbusy = FALSE;
        signal MySerial.sendDone();
    }

    async event void Uart1Interrupts.txDone()
    {
        Scount++;
        if(Scount<Stotalnum)
        {
            call HplUart1.tx(Sbuf[Scount]);
        }
        else 
        { 
            post sendDoneTask();
        }
    }

    task void recevieTask()
    {
        signal MySerial.receive(pprbuf+4,Rlength);
    }

    async event void Uart1Interrupts.rxDone(uint8_t data)
    {
        if(Rcount == 0)
        {
            if(data == 0xAA)
            {
                *(prbuf+Rcount)=data;
                Rsum=data;
                Rcount++;
            }
        }
        else if(Rcount == 1)
        {
            if(data == 0x55)
            {
                *(prbuf+Rcount)=data;
                Rsum+=data;
                Rcount++;
            }
            else if(data != 0xAA)
                Rcount = 0;
        }
        else if(Rcount == 2)
        {
            Rlen = data;
            if(Rlen>RBUF_LENGTH)
                Rcount = 0;
            else
            {
                Rsum += data;
                Rcount++;
            }
        }
        else
        {
            *(prbuf+Rcount)=data;
            Rsum+=data;
            Rcount++;

            if( Rcount >= Rlen )
            {
                Rcount = 0;
                if(Rsum == 0)
                {
                    pprbuf = prbuf;
                    Rlength = Rlen-4;
                    post recevieTask();
                    if(prbuf == Rbuf0)prbuf = Rbuf1;
                    else              prbuf = Rbuf0;
                }
            }
        }
    }
}

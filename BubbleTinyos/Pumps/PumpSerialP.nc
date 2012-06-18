#include "msp430usart.h"

module PumpSerialP
{
  provides interface PumpSerial;

  uses interface HplMsp430Usart as HplUart1;
  uses interface HplMsp430UsartInterrupts as Uart1Interrupts;
}
implementation
{
  enum
  {
    SBUF_LENGTH = 100,
    RBUF_LENGTH = 20
  };
  norace uint8_t Sbuf[SBUF_LENGTH];
  norace uint8_t Scount,Stotalnum;
  norace uint8_t Ssum;
  norace bool    Sbusy;

  norace uint8_t Rbuf0[RBUF_LENGTH];
  norace uint8_t Rbuf1[RBUF_LENGTH];
  norace uint8_t *prbuf,*pprbuf;;
  norace uint8_t Rcount;
  norace uint16_t Rsum;

  async command void PumpSerial.init()
  {
    msp430_uart_union_config_t config =
    {
      {
       utxe : 1,
       urxe : 1,
       ubr : UBR_1MHZ_115200,
       umctl : UMCTL_1MHZ_115200,
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
    Rcount = 0;
    prbuf = Rbuf0;
  }
  
  async command error_t PumpSerial.send(uint8_t *sbuf,uint8_t len)
  {
    if(Sbusy == TRUE)return FAIL;
    if(sbuf == NULL)return FAIL;
    for(Scount=0;Scount<len;Scount++)
    {
      Sbuf[Scount]=*sbuf;
      sbuf++;
    }
    Scount=0;
    Stotalnum = len;
    Sbusy = TRUE;
    call HplUart1.tx(Sbuf[Scount]);
    return SUCCESS;
  }

  task void sendDoneTask()
  {
    Sbusy = FALSE;
    signal PumpSerial.sendDone();
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
    signal PumpSerial.receive(pprbuf);
  }
  
  async event void Uart1Interrupts.rxDone(uint8_t data)
  {
    if(Rcount == 0)
    {
      if(data == 0xAA){*(prbuf+Rcount)=data;Rcount++;}
    }
    else if(Rcount == 1)
    {
      if(data == 0x55){*(prbuf+Rcount)=data;Rcount++;Rsum=0x55AA;}
      else if(data != 0xAA)Rcount = 0;
    }
    else
    {
      *(prbuf+Rcount)=data;

      if(Rcount&1)Rsum += (((uint16_t)data)<<8);
      else        Rsum += data;

      Rcount++;

      if( Rcount >= RBUF_LENGTH )
      {
        Rcount = 0;
        if(Rsum == 0)
        {
          pprbuf = prbuf;
          post recevieTask();
          if(prbuf == Rbuf0)prbuf = Rbuf1;
          else              prbuf = Rbuf0;
        }
      }
    }
  }
}

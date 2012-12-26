// $Id: BlinkC.nc,v 1.5 2008/06/26 03:38:26 regehr Exp $

/*									tab:4
 * "Copyright (c) 2000-2005 The Regents of the University  of California.  
 * All rights reserved.
 *
 * Permission to use, copy, modify, and distribute this software and its
 * documentation for any purpose, without fee, and without written agreement is
 * hereby granted, provided that the above copyright notice, the following
 * two paragraphs and the author appear in all copies of this software.
 * 
 * IN NO EVENT SHALL THE UNIVERSITY OF CALIFORNIA BE LIABLE TO ANY PARTY FOR
 * DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES ARISING OUT
 * OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF THE UNIVERSITY OF
 * CALIFORNIA HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * THE UNIVERSITY OF CALIFORNIA SPECIFICALLY DISCLAIMS ANY WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
 * AND FITNESS FOR A PARTICULAR PURPOSE.  THE SOFTWARE PROVIDED HEREUNDER IS
 * ON AN "AS IS" BASIS, AND THE UNIVERSITY OF CALIFORNIA HAS NO OBLIGATION TO
 * PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS."
 *
 * Copyright (c) 2002-2003 Intel Corporation
 * All rights reserved.
 *
 * This file is distributed under the terms in the attached INTEL-LICENSE     
 * file. If you do not find these files, copies can be found by writing to
 * Intel Research Berkeley, 2150 Shattuck Avenue, Suite 1300, Berkeley, CA, 
 * 94704.  Attention:  Intel License Inquiry.
 */

/**
 * Implementation for Blink application.  Toggle the red LED when a
 * Timer fires.
 **/

#include "Timer.h"
#include "I2C.h"
#include "msp430usart.h"

module ProgramC
{
    uses interface Timer<TMilli> as Timer0;
    uses interface Leds;
    uses interface Boot;

    uses interface MySerial;

    uses interface SplitControl as RadioControl;
    uses interface StdControl as RoutingControl;
    uses interface Receive as CtpReceive;
    uses interface CtpPacket;
    uses interface RootControl;
}
implementation
{
    enum
    {
        UARTBUF_SIZE = 32,
        UARTQUEUE_SIZE = 32,
        RADIOQUEUE_SIZE = 32,
    };

    uint8_t Ubuf[UARTQUEUE_SIZE][UARTBUF_SIZE];
    uint8_t Uin,Uout;
    bool    Ufull,Ubusy;

    message_t Rbuf[RADIOQUEUE_SIZE];
    uint8_t Rlen[RADIOQUEUE_SIZE];
    uint8_t Rin,Rout;
    bool    Rfull,Rbusy;

    event void Boot.booted()
    {
        Uin=0;Uout=0;
        Ufull= FALSE;
        Ubusy = FALSE;
        
        Rin=0;Rout=0;
        Rfull= FALSE;
        Rbusy = FALSE;
        call MySerial.init();
        call RadioControl.start();
    }

    event void RadioControl.startDone(error_t error)
    {
        if(error != SUCCESS)
        {
            call RadioControl.start();
        }
        else 
        {
            call RoutingControl.start();
            call RootControl.setRoot();
            //call Timer0.startPeriodic(4);
            call Timer0.startPeriodic(512);
            //WDTCTL = 0x5A09;
        }
    }

    event void RadioControl.stopDone(error_t error)
    {
    }

    event void Timer0.fired()
    {
        call Leds.led1Toggle();
        //WDTCTL = 0x5A09;
    }

    task void Uart_sendTask()
    {
        atomic
        {
            if(Uin == Uout && Ufull == FALSE)
            {
                Ubusy = FALSE;
                return;
            }
        }
        if(call MySerial.send(Ubuf[Uout],Ubuf[Uout][UARTBUF_SIZE-1]) != SUCCESS)
            post Uart_sendTask();
    }

    event message_t* CtpReceive.receive(message_t* msg, void* payload, uint8_t len)
    {
        uint8_t* p;
        uint8_t  i;
        uint16_t src;
        if(len>UARTBUF_SIZE-1)
        {
            call Leds.led0Toggle();
            return msg;
        }

        src = call CtpPacket.getOrigin(msg);
        if(Ufull == FALSE)
        {
            Ubuf[Uin][0] = src;
            Ubuf[Uin][1] = (src>>8);
            p = (uint8_t*)payload;
            for(i=0;i<len;i++)
            {
                Ubuf[Uin][i+2]=*p++;
            }
            Ubuf[Uin][UARTBUF_SIZE-1]=len+2;

            atomic
            {
                Uin = (Uin+1) % UARTQUEUE_SIZE;
                if(Uin == Uout)
                    Ufull = TRUE;
            }
            if(Ubusy == FALSE)
            {
                post Uart_sendTask();
                Ubusy = TRUE; 
            }
        }
        else
        {
            call Leds.led0Toggle();
        }

        return msg;
    }

    event void MySerial.sendDone()
    {
        atomic
        {
            Uout = (Uout+1) % UARTQUEUE_SIZE;
            Ufull = FALSE;
        }
        post Uart_sendTask();
        call Leds.led2Toggle();
    }
    
    event void MySerial.receive(uint8_t *rbuf,uint8_t len)
    {
    }
}


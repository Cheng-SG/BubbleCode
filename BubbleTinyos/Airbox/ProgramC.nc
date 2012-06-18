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

    uses interface Airbox;

    uses interface SplitControl as RadioControl;
    uses interface Packet;
    uses interface AMPacket;
    uses interface AMSend;
    uses interface Receive;
}
implementation
{
    uint8_t   cnt;
    bool      Rbusy;
    message_t pkt;

    event void Boot.booted()
    {
        call Airbox.init();
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
            call Timer0.startPeriodic(512);
        }
    }

    event void RadioControl.stopDone(error_t error)
    {
    }

    event void Timer0.fired()
    {
        uint8_t* data;
        cnt++;
        call Leds.led1Toggle();
        if(cnt&1)
        {
            if(call Airbox.isOnline()==TRUE)
            {
                data = (uint8_t*)(call Packet.getPayload(&pkt,8));
                *data++ = 0x00;
                *data++ = 0x02;
                call Airbox.read(0,data);
                data++;
                call Airbox.read(1,data);
                data++;
                call Airbox.read(14,data);
                data++;
                call Airbox.read(32,data);
                data++;
                call Airbox.read(33,data);
                data++;
                call Airbox.read(34,data);
                if(SUCCESS == call AMSend.send(0, &pkt, 8))
                {
                    Rbusy = TRUE;
                    call Leds.led2Toggle();
                }
            }
        }
    }

    event void AMSend.sendDone(message_t* msg,error_t error)
    {
        Rbusy = FALSE;
    }

    event message_t* Receive.receive(message_t* msg, void* payload, uint8_t len)
    {
        uint8_t type,length;
        uint8_t address,data;
        uint8_t i,*pdata;

        if(call AMPacket.isForMe(msg)==TRUE)
        {
            pdata = (uint8_t*)payload;
            type = *pdata;
            length = *(pdata+1);
            if(type == 0x02)
            {
                for(i=0;i<length;i++)
                {
                    address = *(pdata+2+i*2);
                    data    = *(pdata+3+i*2);
                    call Airbox.write(address, data);
                }
            }
            call Leds.led0Toggle();
        }
        return msg;
    }

}


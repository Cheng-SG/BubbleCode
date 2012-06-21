README for TelosbSht75
Author/Contact: tinyos-help@millennium.berkeley.edu

Description:

Blink is a simple application that blinks the 3 mote LEDs. It tests
that the boot sequence and millisecond timers are working properly.
The three LEDs blink at 1Hz, 2Hz, and 4Hz. Because each is driven by
an independent timer, visual inspection can determine whether there are
bugs in the timer system that are causing drift. Note that this 
method is different than RadioCountToLeds, which fires a single timer
at a steady rate and uses the bottom three bits of a counter to display
on the LEDs.

Pin assignment:
TelosbPIN     SensironPIN    OldTelosb  NewTelosb 
GIO0(P2.0) ->    VCC         U2 - 10    U2 - 12
GND        ->    GND         U2 - 9     U2 - 11
GIO1(P2.1) ->    DATA        U2 - 7     U2 - 9
GIO2(P2.3) ->    SCK         U28- 2     U28- 2

Tools:

Known bugs/limitations:

None.


$Id: README.txt,v 1.4 2006/12/12 18:22:48 vlahan Exp $

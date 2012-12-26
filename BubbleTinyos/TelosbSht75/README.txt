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

Sensor 1:
TelosbPIN     SensironPIN   NewTelosb 
GIO0(P6.0) ->    VCC         U2 - 3
GIO2(P6.1) ->    SCK         U2 - 5
GIO1(P2.1) ->    DATA        U2 - 9
GND        ->    GND         U2 - 11

Sensor 2:
TelosbPIN     SensironPIN   NewTelosb 
GIO0(P6.2) ->    VCC         U2 - 7
GIO2(P6.3) ->    SCK         U2 - 10
GIO1(P2.0) ->    DATA        U2 - 12
GND        ->    GND         U2 - 11

Tools:

Known bugs/limitations:

None.


$Id: README.txt,v 1.4 2006/12/12 18:22:48 vlahan Exp $

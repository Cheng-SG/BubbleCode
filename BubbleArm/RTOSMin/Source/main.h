#ifndef _MAIN_H_
#define _MAIN_H_

#define LED_PORT 0		// Port for led
#define LED_GREEN_BIT 6 // Bit on port for green led
#define LED_RED_BIT   7 // Bit on port for red led
#define LED_ON 1		// Level to set port to turn on led
#define LED_OFF 0		// Level to set port to turn off led

#define	BLINKY_TASK_PRIORITY	 ( tskIDLE_PRIORITY + 1 )
#define LED_FREQUENCY_MS		 ( 500 / portTICK_RATE_MS )
#define LED_BLINK_PARAMETER		 ( 0x1111UL )



#endif

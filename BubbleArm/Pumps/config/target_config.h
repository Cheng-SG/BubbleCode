/****************************************************************************
 *   $Id:: target_config.h 4832 2010-09-07 23:04:21Z nxp21346                 $
 *   Project: LED flashing / ISP test program
 *
 *   Description:
 *     This file contains the config settings for the blinky code sample
 *     which flashes an LED on the LPCXpresso board and also increments an
 *     LED display on the Embedded Artists base board. This project
 *     implements CRP and is useful for testing bootloaders.
 *
 ****************************************************************************
 * Software that is described herein is for illustrative purposes only
 * which provides customers with programming information regarding the
 * products. This software is supplied "AS IS" without any warranties.
 * NXP Semiconductors assumes no responsibility or liability for the
 * use of the software, conveys no license or title under any patent,
 * copyright, or mask work right to the product. NXP Semiconductors
 * reserves the right to make changes in the software without
 * notification. NXP Semiconductors also make no representation or
 * warranty that such application will be suitable for the specified
 * use without further testing or modification.
****************************************************************************/

#define LED_PORT 0		// Port for led
#define LED_GREEN_BIT 6 // Bit on port for green led
#define LED_RED_BIT   7 // Bit on port for red led
#define LED_ON 1		// Level to set port to turn on led
#define LED_OFF 0		// Level to set port to turn off led

#define SYS_TICK    (SystemCoreClock/8-1)

#define TX_BUFSIZE    64
#define RX_BUFSIZE    64


/*********************************************************************************
**                            End Of File
*********************************************************************************/

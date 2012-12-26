/*
 * GENERATED FILE - DO NOT EDIT
 * (C) Code Red Technologies Ltd, 2008-2012
 * Linker script memory definitions
 * Created from LinkMemoryTemplate
 * By LPCXpresso v5.0.12 [Build 1041] [2012-11-22]  on Mon Dec 17 21:07:12 SGT 2012)
*/

MEMORY
{
  /* Define each memory region */
  MFlash24 (rx) : ORIGIN = 0x0, LENGTH = 0x6000 /* 24k */
  RamLoc8 (rwx) : ORIGIN = 0x10000000, LENGTH = 0x2000 /* 8k */

}
  /* Define a symbol for the top of each memory region */
  __top_MFlash24 = 0x0 + 0x6000;
  __top_RamLoc8 = 0x10000000 + 0x2000;


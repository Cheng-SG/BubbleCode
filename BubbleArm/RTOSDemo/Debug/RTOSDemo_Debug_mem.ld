/*
 * GENERATED FILE - DO NOT EDIT
 * (C) Code Red Technologies Ltd, 2008-2012
 * Linker script memory definitions
 * Created from LinkMemoryTemplate
 * By LPCXpresso v5.0.12 [Build 1041] [2012-11-22]  on Thu Dec 20 21:31:35 SGT 2012)
*/

MEMORY
{
  /* Define each memory region */
  MFlash32 (rx) : ORIGIN = 0x0, LENGTH = 0x8000 /* 32k */
  RamLoc8 (rwx) : ORIGIN = 0x10000000, LENGTH = 0x2000 /* 8k */

}
  /* Define a symbol for the top of each memory region */
  __top_MFlash32 = 0x0 + 0x8000;
  __top_RamLoc8 = 0x10000000 + 0x2000;


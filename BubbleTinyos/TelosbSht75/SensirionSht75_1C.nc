/*
 * Copyright (c) 2005-2006 Arch Rock Corporation
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 * - Redistributions in binary form must reproduce the above copyright
 *   notice, this list of conditions and the following disclaimer in the
 *   documentation and/or other materials provided with the
 *   distribution.
 * - Neither the name of the Arch Rock Corporation nor the names of
 *   its contributors may be used to endorse or promote products derived
 *   from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE
 * ARCHED ROCK OR ITS CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE
 */

/**
 * SensirionSht75C is a top-level access component for the Sensirion
 * SHT75 model humidity and temperature sensor, available on the
 * telosb platform. Because this component represents one physical
 * device, simultaneous calls to read temperature and humidity will be
 * arbitrated and executed in sequential order. Feel free to read both
 * at the same time, just be aware that they'll come back
 * sequentially.
 *
 * @author Gilman Tolle <gtolle@archrock.com>
 * @version $Revision: 1.5 $ $Date: 2007-04-13 21:46:18 $
 */

generic configuration SensirionSht75_1C() {
  provides interface Read<uint16_t> as Temperature;
  provides interface DeviceMetadata as TemperatureMetadata;
  provides interface Read<uint16_t> as Humidity;
  provides interface DeviceMetadata as HumidityMetadata;
}
implementation {
  components new SensirionSht75ReaderP();

  Temperature = SensirionSht75ReaderP.Temperature;
  TemperatureMetadata = SensirionSht75ReaderP.TemperatureMetadata;
  Humidity = SensirionSht75ReaderP.Humidity;
  HumidityMetadata = SensirionSht75ReaderP.HumidityMetadata;

  components HalSensirionSht75_1C;

  enum { TEMP_KEY = unique("Sht75_1.Resource") };
  enum { HUM_KEY = unique("Sht75_1.Resource") };

  SensirionSht75ReaderP.TempResource -> HalSensirionSht75_1C.Resource[ TEMP_KEY ];
  SensirionSht75ReaderP.Sht75Temp -> HalSensirionSht75_1C.SensirionSht75[ TEMP_KEY ];
  SensirionSht75ReaderP.HumResource -> HalSensirionSht75_1C.Resource[ HUM_KEY ];
  SensirionSht75ReaderP.Sht75Hum -> HalSensirionSht75_1C.SensirionSht75[ HUM_KEY ];
}

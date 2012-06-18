interface PumpSerial
{
  async command void init();
  async command error_t send(uint8_t *sbuf,uint8_t len);
  event void sendDone();
  event void receive(uint8_t *rbuf);
}

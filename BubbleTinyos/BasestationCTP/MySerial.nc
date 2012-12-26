interface MySerial
{
    command void init();
    command error_t send(uint8_t *sbuf,uint8_t len);
    event void sendDone();
    event void receive(uint8_t *payload,uint8_t len);
}

interface Airbox
{
    command void init(void);
    async command bool isOnline(void);
    async command bool read(uint8_t address,uint8_t* data);
    async command bool write(uint8_t address,uint8_t data);   
}

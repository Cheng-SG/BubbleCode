import sys
try:
    import serial
except:
    print "You don't have serial module installed. Please install pySerial firrst!"
    sys.exit(1)

class Packet:
    Txlenth = 0
    ser  = ''
    MaxPacketLength = 64
    TxPacket = [0xAA,0x55,0x00,0xFF]

    def __init__(self,port,baudrate=115200,tout=None):
        self.ser = serial.Serial(port,baudrate,timeout=tout)

    def send(self,payload):
        if (len(payload)+4) >self.MaxPacketLength:
            return
        self.TxPacket[0] = 0xAA
        self.TxPacket[1] = 0x55
        self.TxPacket[2] = len(payload)+4
        self.TxPacket[3] = 0x00
        for i in range(len(payload)):
            payload[i] %= 256
        self.TxPacket[4:] = payload[:]
        self.TxPacket[3] = 256 - sum(self.TxPacket)%256
        self.TxPacket[3] %= 256
        mstr = []
        for m in self.TxPacket:
            mstr.append(chr(m))
        fstr = ''.join(mstr)
        self.ser.write(fstr)

    def receive(self):
        RxCount = 0
        RxLength = 0
        RxSum = 0
        Payload = []
        RxPacket = range(self.MaxPacketLength)
        while True:
            data = self.ser.read(1)
            if data == '':
                continue
            if RxCount==0:
                if data=='\xAA':
                    RxSum = ord(data)
                    RxPacket[RxCount] = ord(data)
                    RxCount += 1
            elif RxCount==1:
                if data =='\x55':
                    RxSum += ord(data)
                    RxPacket[RxCount] = ord(data)
                    RxCount +=1
                elif data != 'xAA':
                    RxCount = 0
                    RxLength = 0
            elif RxCount==2:
                RxLength = ord(data)
                RxSum += ord(data)
                RxPacket[RxCount] = ord(data)
                RxCount += 1
            elif RxCount < RxLength:
                RxSum += ord(data)
                RxPacket[RxCount] = ord(data)
                RxCount += 1
                if RxCount == RxLength:
                    RxSum %= 256
                    if RxSum == 0:
                        Payload[:] = RxPacket[4:RxLength]
                        break
                    RxCount = 0
                    RxLength = 0
        return Payload

    def FlushInBuff(self):
        self.ser.flushInput()

if __name__ == "__main__":
    import code
    import time
    import random

    def Count(pkt):
        pkt.FlushInBuff()
        m = pkt.receive()
        Scount = m[2] + m[3]*256
        Rcount = m[4] + m[5]*256
        print Scount,Rcount

    def Send(pkt,num):
        for i in range(num):
            m = []
            for j in range(16):
                m.append((int)(random.random()*256))
            pkt.send(m)
            #time.sleep(0.02)

    if len(sys.argv) != 2:
        print "Usage:python {0} port".format(sys.argv[0])
        sys.exit(1)
    try:
        pkt = Packet(sys.argv[1])
    except:
        print "Error: invalid port. Check the port is right"
        sys.exit(1)

    print '\n'
    print "*******************************************************"
    print 'Usage:\n    pkt.send(content) to send a packet'
    print '\n    pkt.receive() to receive a packet'
    print 'example:\n    send a packet'
    print '    >>>content = [1,0,0,1,2,3]'
    print '    >>>pkt.send(content)\n'
    print '    receive a packet'
    print '    >>>pkt.receive()'
    print '    you may get this: [1, 0, 0, 4, 96, 28, 243, 6]'
    print "*******************************************************"
    print '\n'
    content = [11,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]
    code.InteractiveConsole(locals=globals()).interact()





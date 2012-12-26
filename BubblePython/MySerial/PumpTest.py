import time
from threading import Thread
import sys,datetime
import MySerial as P

setpoints = [0] * 8
flowrates = [0] * 8
configures = [2200,2200,4700,4700,752,752,4700,4700]
channel = 0
ThreadEn = 0
recordEn = 0

def Record(pkt,f):
    while ThreadEn==1:
        tmp = pkt.receive()
        if len(tmp) == 20:
            nodeID = tmp[0] + (tmp[1]<<8)
            dataType = tmp[2] + (tmp[3]<<8)
            if nodeID == 12 and dataType == 0x0100:
                for i in range(8):
                    flowrates[i] = tmp[i*2+4] + (tmp[i*2+5]<<8)
                    flowrates[i] = flowrates[i] * 500.0 / configures[i];
                out = '{0}:'.format(channel+1)
                out += datetime.datetime.strftime(datetime.datetime.now(),'%Y-%m-%d %H:%M:%S,')
                if channel<4:
                    out += '{0:0.3f},{1:0.3f}\n'.format(setpoints[channel],flowrates[channel/2])
                else:
                    out += '{0:0.3f},{1:0.3f}\n'.format(setpoints[channel],flowrates[channel-2])
                if recordEn == 1:
                    f.write(out)
                    #time.sleep(1)
                    #pkt.FlushInBuff()

def SendSpeeds(pkt):
    tmp = [12,0,1,1];
    for k in range(8):
        m = 65535 - setpoints[k]
        tmp.append(m%256)
        tmp.append(m/256)
    pkt.send(tmp)

if len(sys.argv) != 3:
    print "Usage:python {0} Port filename".format(sys.argv[0])
    print "Example:python {0} /dev/ttyS1 record1.txt".format(sys.argv[0])
    sys.exit(1)

try:
    pkt = P.Packet(sys.argv[1],115200,0.01)
except:
    print "Error: unable to open port {0}, check it availability".format(sys.argv[1])
    sys.exit()

try:
    f = open(sys.argv[2],'a+')
except:
    print "Error: can't open or create file {0}".format(sys.argv[2])
    sys.exit(1)

t = Thread(target=Record, args=(pkt,f))
ThreadEn = 1
t.start()

print 'Testing running. To terminate, press Control+C'
try:
    for i in range(6):
        channel = i
        recordEn = 0
        setpoints[i] = 32768
        SendSpeeds(pkt)
        time.sleep(30)
        recordEn = 1
        for j in range(0,65535,1000):
            setpoints[i] = j;
            SendSpeeds(pkt)
            time.sleep(5)
        setpoints[i] = 0
except:
    ThreadEn = 0;
    t.join()
    sys.exit(1)


SendSpeeds(pkt)
recordEn = 0
ThreadEn = 0;
t.join()

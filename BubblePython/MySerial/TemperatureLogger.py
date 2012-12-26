import sys,math,datetime
import MySerial as P

if len(sys.argv) != 3:
    print "Usage:python {0} Port filename".format(sys.argv[0])
    print "Example:python {0} /dev/ttyS1 record1.txt".format(sys.argv[0])
    sys.exit(1)

try:
    pkt = P.Packet(sys.argv[1])
except:
    print "Error: unable to open port {0}, check it availability".format(sys.argv[1])
    sys.exit()

try:
    f = open(sys.argv[2],'a+')
except:
    print "Error: can't open or create file {0}".format(sys.argv[2])
    sys.exit(1)
    
try:
    while True:
        tmp = pkt.receive()
        if len(tmp) == 8:
            nodeID = tmp[0] + (tmp[1]<<8)
            dataType = tmp[2] + (tmp[3]<<8)
            if dataType == 0x0400:
                temperature = tmp[4] + (tmp[5]<<8)
                humidityT = tmp[6] + (tmp[7]<<8)
                temperature = -39.4 + 0.01 * temperature
                humidity = -2.0468 + 0.0367 * humidityT - 0.00000015955 * humidityT * humidityT
                humidity = (temperature - 25) * (0.01 + 0.00008 * humidityT) + humidity
                dewpoint = temperature
                if humidity>100.0:
                    humidity = 100.0
                    dewpoint = temperature
                else:
                    dewpoint = 243.12 * (math.log(humidity / 100.0) + 17.62 * temperature / (243.12 + temperature))  / (17.62 - math.log(humidity / 100) - 17.62 * temperature / (243.12 + temperature))
                out = '{0}:'.format(nodeID)
                out += datetime.datetime.strftime(datetime.datetime.now(),'%Y-%m-%d %H:%M:%S,')
                out += '{0:0.3f},{1:0.3f},{2:0.3f}\n'.format(temperature,humidity,dewpoint)
                f.write(out)
except:
    f.close()
    sys.exit()

import sys
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
        f.write(str(tmp)+'\n')
except:
    f.close()
    sys.exit()

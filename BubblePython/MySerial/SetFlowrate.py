import sys
import MySerial as P

setpoints = [0] * 4
configures = [2200,2200,4700,4700]

def SendSpeeds(pkt):
    tmp = [12,0,1,1];
    for k in range(4):
        m = setpoints[k]
        tmp.append(m%256)
        tmp.append(m/256)
        tmp.append(m/65536)
        tmp.append(m/16777216)
    pkt.send(tmp)
    
def SetSpeed(pkt,ch,value):
    if ch>0:
        if ch<5:
            ch = ch - 1
            value = value*configures[ch]*2/1000;
            setpoints[ch] = value
            SendSpeeds(pkt)

if __name__ == "__main__":
    import code
    if len(sys.argv) != 2:
        print "Usage:python {0} port".format(sys.argv[0])
        sys.exit(1)
    try:
        pkt = P.Packet(sys.argv[1])
    except:
        print "Error: invalid port. Check the port is right"
        sys.exit(1)
        
    code.InteractiveConsole(locals=globals()).interact() 
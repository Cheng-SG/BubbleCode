using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BubbleDataCollection
{
    enum SensorType
    {
        TEMPBOARD = 0,
        PUMPBOARD = 1,
        AIRBOX     = 2,
        CO2FLAP    = 3,
        TELOSB     = 4
    };

    public class TemperatueBoard
    {
        UInt16 id;
        int type;
        bool online, online_t;
        UInt16[] temperature = new UInt16[16];
        bool newindata;
        bool uploadcreatenewfile;

        public TemperatueBoard()
	    {
            type = (int)SensorType.TEMPBOARD;
            online = false;
            online_t = false;
            for (int i = 0; i < 16; i++)
                temperature[i] = 0x4000;
            newindata = false;
            uploadcreatenewfile = true;
	    }

        public TemperatueBoard(UInt16 Id)
        {
            id = Id;
            type = (int)SensorType.TEMPBOARD;
            online = true;
            online_t = false;
            for (int i = 0; i < 16; i++)
                temperature[i] = 0x4000;
            newindata = false;
            uploadcreatenewfile = true;
        }

        public UInt16  ID 
        {
            get { return id; }
            set { id = value; } 
        }

        public int Type
        {
            get { return type; }            
        }

        public bool Online
        {
            get { return online; }
            set { online = value; }
        }

        public bool Online_T
        {
            get { return online_t; }
            set { online_t = value; }
        }

        public bool NewInData
        {
            get { return newindata; }
            set { newindata = value; }
        }

        public bool UploadCreateNewFile
        {
            get { return uploadcreatenewfile; }
            set { uploadcreatenewfile = value; }
        }

        public override bool Equals(object obj)
        {     
            return (id == (UInt16)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool SetTemperature(byte[] Buf, int half)
        {
            bool ret = false;
            try
            {
                if (half == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        temperature[i] = (UInt16)((UInt16)(((UInt16)Buf[2 * i + 5]) << 8) + (UInt16)Buf[2 * i + 4]);
                    }
                    ret = true;
                }
                else if (half == 1)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        temperature[i + 8] = (UInt16)((UInt16)(((UInt16)Buf[2 * i + 5]) << 8) + (UInt16)Buf[2 * i + 4]);
                    }
                    newindata = true;
                    ret = true;
                }
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        public UInt16[] GetTemperature()
        {
            return temperature;
        }
    }

    public class PumpBoard
    {
        UInt16 id;
        int type;
        bool online,online_t;
        UInt16[] flowrate = new UInt16[8];
        bool newindata;
        int[] pumpvalue = new int[8];
        UInt16[] pumpspeed = new UInt16[8];
        bool newoutdata;
        bool uploadcreatenewfile;

        public PumpBoard()
        {
            type = (int)SensorType.PUMPBOARD;
            online = false;
            online_t = false;            
            for (int i = 0; i < 8; i++)
                flowrate[i] = 0;
            newindata = false;
            for (int i = 0; i < 8; i++)
                pumpvalue[i] = 0;
            for (int i = 0; i < 8; i++)
                pumpspeed [i] = 65535;
            newoutdata = false;
            uploadcreatenewfile = true;
        }

        public PumpBoard(UInt16 Id)
        {
            id = Id;
            type = (int)SensorType.PUMPBOARD;
            online = false;
            online_t = false;
            for (int i = 0; i < 8; i++)
                flowrate[i] = 0;
            newindata = false;
            for (int i = 0; i < 8; i++)
                pumpvalue[i] = 0;
            for (int i = 0; i < 8; i++)
                pumpspeed[i] = 65535;
            newoutdata = false;
            uploadcreatenewfile = true;
        }

        public UInt16 ID
        {
            get { return id; }
            set { id = value; }
        }

        public int Type
        {
            get { return type; }
        }

        public bool Online
        {
            get { return online; }
            set { online = value; }
        }

        public bool Online_T
        {
            get { return online_t; }
            set { online_t = value; }
        }

        public bool NewInData
        {
            get { return newindata; }
            set { newindata = value; }
        }

        public bool NewOutData
        {
            get { return newoutdata; }
            set { newoutdata = value; }
        }

        public bool UploadCreateNewFile
        {
            get { return uploadcreatenewfile; }
            set { uploadcreatenewfile = value; }
        }

        public override bool Equals(object obj)
        {
            return (id == (UInt16)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool SetFlowrate(byte[] Buf)
        {
            bool ret = false;
            try
            {                
                for (int i = 0; i < 8; i++)
                {
                    flowrate[i] = (UInt16)((UInt16)(((UInt16)Buf[2 * i + 9]) << 5) + (UInt16)Buf[2 * i + 4]);
                }
                newindata = true;
                ret = true;               
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        public UInt16[] GetFlowrate()
        {
            return flowrate;
        }

        public bool SetPumpvalue(int value, int ch)
        {
            bool ret = false;
            if (ch > 0 && ch < 9)
            {
                if (value >= 0 && value <= 10)
                {
                    pumpvalue[ch - 1] = value;
                    pumpspeed[ch - 1] = (UInt16)((10.0 - value) * 65535.0 / 10.0);
                    newoutdata = true;
                    ret = true;
                }
            }
            return ret;
        }

        public int[] GetPumpvalue()
        {
            return pumpvalue;
        }        

        public UInt16[] GetPumpspeed()
        {
            return pumpspeed;
        }
    }

    public class Airbox
    {
        UInt16 id;
        int type;
        bool online, online_t;
        byte[] airboxdata = new byte[6];
        bool newindata;
        int airboxvalue;
        byte airboxspeed;
        bool newoutdata;
        bool uploadcreatenewfile;

        public Airbox()
        {
            type = (int)SensorType.AIRBOX;
            online = false;
            online_t = false;
            for (int i = 0; i < 6; i++)
                airboxdata[i] = 0;
            newindata = false;
            airboxvalue = 0;
            newoutdata = false;
            uploadcreatenewfile = true;
        }

        public Airbox(UInt16 Id)
        {
            id = Id;
            type = (int)SensorType.AIRBOX;
            online = true;
            online_t = false;
            for (int i = 0; i < 6; i++)
                airboxdata[i] = 0;
            newindata = false;
            airboxvalue = 0;
            newoutdata = false;
            uploadcreatenewfile = true;
        }

        public UInt16 ID
        {
            get { return id; }
            set { id = value; }
        }

        public int Type
        {
            get { return type; }
        }

        public bool Online
        {
            get { return online; }
            set { online = value; }
        }

        public bool Online_T
        {
            get { return online_t; }
            set { online_t = value; }
        }

        public byte Speed
        {
            get { return airboxspeed; }
        }

        public bool NewInData
        {
            get { return newindata; }
            set { newindata = value; }
        }

        public bool NewOutData
        {
            get { return newoutdata; }
            set { newoutdata = value; }
        }

        public bool UploadCreateNewFile
        {
            get { return uploadcreatenewfile; }
            set { uploadcreatenewfile = value; }
        }

        public override bool Equals(object obj)
        {
            return (id == (UInt16)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool SetAirboxdata(byte[] Buf)
        {
            bool ret = false;
            try
            {
                for (int i = 0; i < 6; i++)
                    airboxdata[i] = Buf[i + 4];
                newindata = true;
                ret = true;
            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        public byte[] GetAirboxdata()
        {
            return airboxdata;
        }

        public bool SetAirboxvalue(int value)
        {
            bool ret = false;
            if (value >= 0 && value <= 10)
            {
                airboxvalue = value;
                if (value == 0) airboxspeed = 1;
                else airboxspeed = (byte)(value * 10 + 100);
                newoutdata = true;
                ret = true;
            }
            return ret;
        }
    }

    public class CO2flap
    {
        UInt16 id;
        int type;
        bool online, online_t;
        byte[] CO2flapdata = new byte[10];
        byte[] CO2outdata = new byte[10];
        bool newindata;
        bool newoutdata;
        bool uploadcreatenewfile;

        public CO2flap()
        {
            type = (int)SensorType.CO2FLAP;
            online = false;
            online_t = false;
            for (int i = 0; i < 10; i++)
                CO2flapdata[i] = 0;
            newindata = false;
            CO2outdata[0] = 0x01;
            CO2outdata[1] = 0x04;
            CO2outdata[2] = 10;
            CO2outdata[3] = 90;
            CO2outdata[4] = 11;
            CO2outdata[5] = 30;
            CO2outdata[6] = 12;
            CO2outdata[7] = 150;
            CO2outdata[8] = 13;
            CO2outdata[9] = 10;
            newoutdata = false;
            uploadcreatenewfile = true;
        }

        public CO2flap(UInt16 Id)
        {
            id = Id;
            type = (int)SensorType.CO2FLAP;
            online = true;
            online_t = false;
            for (int i = 0; i < 10; i++)
                CO2flapdata[i] = 0;
            newindata = false;
            CO2outdata[0] = 0x01;
            CO2outdata[1] = 0x04;
            CO2outdata[2] = 10;
            CO2outdata[3] = 90;
            CO2outdata[4] = 11;
            CO2outdata[5] = 30;
            CO2outdata[6] = 12;
            CO2outdata[7] = 150;
            CO2outdata[8] = 13;
            CO2outdata[9] = 10;
            newoutdata = false;
            uploadcreatenewfile = true;
        }

        public int Type
        {
            get { return type; }
        }

        public UInt16 ID
        {
            get { return id; }
            set { id = value; }
        }

        public bool Online
        {
            get { return online; }
            set { online = value; }
        }

        public bool Online_T
        {
            get { return online_t; }
            set { online_t = value; }
        }

        public bool NewInData
        {
            get { return newindata; }
            set { newindata = value; }
        }

        public bool NewOutData
        {
            get { return newoutdata; }
            set { newoutdata = value; }
        }

        public bool UploadCreateNewFile
        {
            get { return uploadcreatenewfile; }
            set { uploadcreatenewfile = value; }
        }

        public override bool Equals(object obj)
        {
            return (id == (UInt16)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool SetCO2flapdata(byte[] Buf)
        {
            bool ret = false;
            try
            {
                for (int i = 0; i < 10; i++)
                    CO2flapdata[i] = Buf[i + 4];
                newindata = true;
                ret = true;
            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        public byte[] GetCO2flapdata()
        {
            return CO2flapdata;
        }

        public bool SetCO2flapParam(byte data, int index)
        {
            bool ret = false;
            if(index>=0 && index<=4)
            {
                if (CO2outdata[2 * index + 1] != data)
                {
                    CO2outdata[2 * index + 1] = data;
                    newoutdata = true;
                    ret = true;
                }
            }
            return ret;
        }

        public byte[] GetCO2flapParam()
        {
            return CO2outdata;
        }
    }

    public class TelosbSensor
    {
        UInt16 id;
        int type;
        bool online, online_t;
        UInt16[] telosbdata = new UInt16[2];
        double temperature,humidity,dewpoint;
        bool newindata;
        bool uploadcreatenewfile;
       

        public TelosbSensor()
        {
            type = (int)SensorType.TELOSB;
            online = false;
            online_t = false;
            for (int i = 0; i < 2; i++)
                telosbdata[i] = 0;
            temperature = 0.0;
            humidity = 0.0;
            newindata = false;
            uploadcreatenewfile = true;
        }

        public TelosbSensor(UInt16 Id)
        {
            id = Id;
            type = (int)SensorType.TELOSB;
            online = true;
            online_t = false;
            for (int i = 0; i < 2; i++)
                telosbdata[i] = 0;
            temperature = 0.0;
            humidity = 0.0;
            newindata = false;
            uploadcreatenewfile = true;
        }

        public UInt16 ID
        {
            get { return id; }
            set { id = value; }
        }

        public int Type
        {
            get { return type; }
        }

        public bool Online
        {
            get { return online; }
            set { online = value; }
        }

        public bool Online_T
        {
            get { return online_t; }
            set { online_t = value; }
        }

        public bool NewInData
        {
            get { return newindata; }
            set { newindata = value; }
        }

        public double Temperature
        {
            get { return temperature; }
        }

        public double Humidity
        {
            get { return humidity; }
        }

        public double DewPoint
        {
            get { return dewpoint; }
        }

        public bool UploadCreateNewFile
        {
            get { return uploadcreatenewfile; }
            set { uploadcreatenewfile = value; }
        }

        public override bool Equals(object obj)
        {
            return (id == (UInt16)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool SetTelosbdata(byte[] Buf)
        {
            bool ret = false;
            try
            {
                for (int i = 0; i < 2; i++)
                    telosbdata[i] = (UInt16)((UInt16)(((UInt16)Buf[2 * i + 5]) << 8) + (UInt16)Buf[2 * i + 4]);
                temperature = -39.4 + 0.01 * telosbdata[0];
                humidity = -2.0468 + 0.0367 * telosbdata[1] - 0.00000015955 * telosbdata[1] * telosbdata[1];
                humidity = (temperature - 25) * (0.01 + 0.00008 * telosbdata[1]) + humidity;
                if (temperature > 0)
                {
                    if (humidity >= 100.0)
                    {
                        humidity = 100.0;
                        dewpoint = temperature;
                    }
                    else
                    {
                        dewpoint = 243.12 * (Math.Log(humidity / 100.0) + 17.62 * temperature / (243.12 + temperature)) /
                            (17.62 - Math.Log(humidity / 100) - 17.62 * temperature / (243.12 + temperature));
                    }
                }
                else
                {
                    if (humidity >= 100.0)
                    {
                        humidity = 100.0;
                        dewpoint = temperature;
                    }
                    else
                    {
                        dewpoint = 272.62 * (Math.Log(humidity / 100.0) + 22.46 * temperature / (22.46 + temperature)) /
                            (22.46 - Math.Log(humidity / 100) - 22.46 * temperature / (22.46 + temperature));
                    }
                }
                newindata = true;
                ret = true;
            }
            catch
            {
                ret = false;
            }
            return ret;
        }
  
    }
}

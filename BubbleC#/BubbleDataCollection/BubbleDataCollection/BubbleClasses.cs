﻿using System;
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

    public class BoardConfig
    {
        UInt16 id;
        public int type;
        string[] config = new string[16];

        public BoardConfig(UInt16 Id, int Type)
        {
            id = Id;
            type = Type;
            for (int i = 0; i < 16; i++)
                config[i] = "NA";
        }

        public override bool Equals(object obj)
        {
            return (id == (UInt16)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool SetConfig(string[] cfgs)
        {
            if (cfgs.Length == 4)
            {
                try
                {
                    int Id = int.Parse(cfgs[0]);
                    int Type = -1;

                    if (cfgs[1] == "TEMPERATURE")
                        Type = (int)SensorType.TEMPBOARD;
                    else if (cfgs[1] == "FLOWRATE" || cfgs[1] == "PUMP")
                        Type = (int)SensorType.PUMPBOARD;
                    else
                        return false;

                    if (Type != type)
                        return false;

                    int chanel;
                    if (cfgs[2].StartsWith("CH"))
                    {
                        chanel = int.Parse(cfgs[2].Substring(2));
                        if (cfgs[1] == "TEMPERATURE")
                            chanel = chanel - 1;
                        else if (cfgs[1] == "FLOWRATE")
                            chanel = chanel - 1;
                        else if (cfgs[1] == "PUMP")
                            chanel = chanel + 7;
                    }
                    else
                        return false;

                    config[chanel] = cfgs[3];
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public string[] GetConfig()
        {
            return config;
        }
    }

    public class TemperatureConfig
    {
        public UInt16 id;
        public int[] config = new int[16];

        public override bool Equals(object obj)
        {
            return (id == (UInt16)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class TemperatueBoard
    {
        UInt16 id;
        int type;
        int[] config = new int[16];
        bool online, online_t;
        int port;
        UInt16[] temperature = new UInt16[16];
        bool newindata;
        bool uploadcreatenewfile;

        public TemperatueBoard()
	    {
            type = (int)SensorType.TEMPBOARD;
            online = false;
            online_t = false;
            port = -1;
            for (int i = 0; i < 16; i++)
            {
                temperature[i] = 0x4000;
                config[i] = 0;
            }
            newindata = false;
            uploadcreatenewfile = true;
	    }

        public TemperatueBoard(UInt16 Id)
        {
            id = Id;
            type = (int)SensorType.TEMPBOARD;
            online = true;
            online_t = false;
            port = -1;
            for (int i = 0; i < 16; i++)
            {
                temperature[i] = 0x4000;
                config[i] = 0;
            }
            newindata = false;
            uploadcreatenewfile = true;
        }
        public TemperatueBoard(UInt16 Id, TemperatureConfig newconfig)
        {
            id = Id;
            type = (int)SensorType.TEMPBOARD;
            online = true;
            online_t = false;
            port = -1;
            for (int i = 0; i < 16; i++)
            {
                temperature[i] = 0x4000;
                if(Id == newconfig.id)
                    config[i] = newconfig.config[i];
            }
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

        public int Port
        {
            get { return port; }
            set { port = value; }
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

        public bool SetTemperature(byte[] Buf)
        {
            bool ret = false;
            try
            {
                    for (int i = 0; i < 16; i++)
                    {
                        temperature[i] = (UInt16)((UInt16)(((UInt16)Buf[2 * i + 5]) << 8) + (UInt16)Buf[2 * i + 4]);
                        if (temperature[i] != 0x4000)
                            temperature[i] = (UInt16)(temperature[i] + config[i]);
                    }
                    ret = true;
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        public int[] GetConfig()
        {
            return config;
        }

        public bool SetConfig(int CH, int value)
        {
            if (CH > 0 & CH < 17)
            {
                config[CH - 1] = value;
                return true;
            }
            return false;
        }

        public bool SetConfig(int[] values)
        {
            if (values.Length == 16)
            {
                for (int i = 0; i < 16; i++)
                    config[i] = values[i];
                return true;
            }
            return false;
        }

        public UInt16[] GetTemperature()
        {
            return temperature;
        }
    }

    public class FlowrateConfig
    {
        public UInt16 id;
        public int[] config = new int[8];

        public override bool Equals(object obj)
        {
            return (id == (UInt16)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public class PumpBoard
    {
        UInt16 id;
        int type;
        int[] config = new int[8];
        bool online,online_t;
        int port;
        double[] flowrate = new double[8];
        bool newindata;
        double[] pumpvalue = new double[8];
        Int16[] pumpspeed = new Int16[8];
        bool newoutdata;
        bool uploadcreatenewfile;

        public PumpBoard()
        {
            type = (int)SensorType.PUMPBOARD;
            online = false;
            online_t = false;
            port = -1;
            for (int i = 0; i < 8; i++)
            {
                flowrate[i] = 0;
                config[i] = 2;
            }
            newindata = false;
            for (int i = 0; i < 8; i++)
                pumpvalue[i] = 0;
            for (int i = 0; i < 8; i++)
                pumpspeed [i] = 0;
            newoutdata = false;
            uploadcreatenewfile = true;
        }

        public PumpBoard(UInt16 Id)
        {
            id = Id;
            type = (int)SensorType.PUMPBOARD;
            online = false;
            online_t = false;
            port = -1;
            for (int i = 0; i < 8; i++)
            {
                flowrate[i] = 0;
                config[i] = 2;
            }   
            newindata = false;
            for (int i = 0; i < 8; i++)
                pumpvalue[i] = 0;
            for (int i = 0; i < 8; i++)
                pumpspeed[i] = 0;
            newoutdata = false;
            uploadcreatenewfile = true;
        }

        public PumpBoard(UInt16 Id,FlowrateConfig newconfig)
        {
            id = Id;
            type = (int)SensorType.PUMPBOARD;
            online = false;
            online_t = false;
            port = -1;
            for (int i = 0; i < 8; i++)
            {
                flowrate[i] = 0;
                if (Id == newconfig.id)
                    config[i] = newconfig.config[i];
            }
            newindata = false;
            for (int i = 0; i < 8; i++)
                pumpvalue[i] = 0;
            for (int i = 0; i < 8; i++)
                pumpspeed[i] = 0;
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

        public int Port
        {
            get { return port; }
            set { port = value; }
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

        public int[] GetConfig()
        {
            return config;
        }

        public bool SetConfig(int CH, int value)
        {
            if (CH > 0 & CH < 9)
            {
                config[CH - 1] = value;
                return true;
            }
            return false;
        }

        public bool SetConfig(int[] values)
        {
            if (values.Length == 8)
            {
                for (int i = 0; i < 8; i++)
                    config[i] = values[i];
                return true;
            }
            return false;
        }

        public bool SetFlowrate(byte[] Buf)
        {
            bool ret = false;
            try
            {                
                for (int i = 0; i < 8; i++)
                {
                    flowrate[i] = (Int16)((UInt16)(((UInt16)Buf[2 * i + 5]) << 8) + (UInt16)Buf[2 * i + 4]);
                    if (config[i] != 0)
                        flowrate[i] = flowrate[i] * 500.0 / config[i];
                    else
                        flowrate[i] = flowrate[i] / 2;
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

        public double[] GetFlowrate()
        {
            return flowrate;
        }

        public bool SetPumpvalue(double value, int ch)
        {
            bool ret = false;
            if (ch > 0 && ch < 9)
            {
                if (value >= 0 && value <= 10)
                {
                    pumpvalue[ch - 1] = value;
                    pumpspeed[ch - 1] = (Int16)((value) * 65535.0 / 10.0);
                    newoutdata = true;
                    ret = true;
                }
            }
            return ret;
        }

        public double[] GetPumpvalue()
        {
            return pumpvalue;
        }        

        public Int16[] GetPumpspeed()
        {
            return pumpspeed;
        }
    }

    public class Airbox
    {
        UInt16 id;
        int type;
        bool online, online_t;
        int port;
        byte[] airboxdata = new byte[6];
        bool newindata;
        int airboxvalue;
        byte airboxspeed;
        bool newoutdata;
        bool uploadcreatenewfile;
        TelosbSensor InputSensor;
        TelosbSensor OutputSensor;

        public Airbox()
        {
            type = (int)SensorType.AIRBOX;
            online = false;
            online_t = false;
            port = -1;
            for (int i = 0; i < 6; i++)
                airboxdata[i] = 0;
            newindata = false;
            airboxvalue = 0;
            newoutdata = false;
            uploadcreatenewfile = true;
            InputSensor = new TelosbSensor();
            OutputSensor = new TelosbSensor();
        }

        public Airbox(UInt16 Id)
        {
            id = Id;
            type = (int)SensorType.AIRBOX;
            online = true;
            online_t = false;
            port = -1;
            for (int i = 0; i < 6; i++)
                airboxdata[i] = 0;
            newindata = false;
            airboxvalue = 0;
            newoutdata = false;
            uploadcreatenewfile = true;
            InputSensor = new TelosbSensor();
            OutputSensor = new TelosbSensor();
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

        public int Port
        {
            get { return port; }
            set { port = value; }
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
        bool flapauto;
        int port;        
        byte[] CO2flapdata = new byte[12];
        byte[] CO2outdata = new byte[12];
        bool newindata;
        bool newoutdata;
        bool uploadcreatenewfile;

        public CO2flap()
        {
            type = (int)SensorType.CO2FLAP;
            online = false;
            online_t = false;
            flapauto = false;
            port = -1;
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
            CO2outdata[10] = 14;
            CO2outdata[11] = 0;
            newoutdata = false;
            uploadcreatenewfile = true;
        }

        public CO2flap(UInt16 Id)
        {
            id = Id;
            type = (int)SensorType.CO2FLAP;
            online = true;
            online_t = false;
            flapauto = false;
            port = -1;
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
            CO2outdata[10] = 14;
            CO2outdata[11] = 0;
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

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public bool FlapAuto
        {
            get { return flapauto; }
            set 
            { 
                flapauto = value;
                if (flapauto == true)
                    CO2outdata[11] = 0xE0;
                else
                    CO2outdata[11] = 0x00;
                newoutdata = true;
            }
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

    public class TelosbConfig
    {
        public UInt16 id;
        public int config;

        public override bool Equals(object obj)
        {
            return (id == (UInt16)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class TelosbSensor
    {
        UInt16 id;
        int type;
        int config;
        bool online, online_t;
        int port;
        UInt16[] telosbdata = new UInt16[2];
        double temperature,humidity,dewpoint;
        bool newindata;
        bool uploadcreatenewfile;
       

        public TelosbSensor()
        {
            type = (int)SensorType.TELOSB;
            online = false;
            online_t = false;
            port = -1;
            for (int i = 0; i < 2; i++)
                telosbdata[i] = 0;
            config = 0;
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
            port = -1;
            for (int i = 0; i < 2; i++)
                telosbdata[i] = 0;
            config = 0;
            temperature = 0.0;
            humidity = 0.0;
            newindata = false;
            uploadcreatenewfile = true;
        }

        public TelosbSensor(UInt16 Id,TelosbConfig newconfig)
        {
            id = Id;
            type = (int)SensorType.TELOSB;
            online = true;
            online_t = false;
            port = -1;
            for (int i = 0; i < 2; i++)
                telosbdata[i] = 0;
            if (Id == newconfig.id)
                config = newconfig.config;
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

        public int Port
        {
            get { return port; }
            set { port = value; }
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

        public int GetConfig()
        {
            return config;
        }

        public bool SetConfig(int value)
        {
            
            config = value;
            return true;
           
        }

        public bool SetTelosbdata(byte[] Buf)
        {
            bool ret = false;
            try
            {
                for (int i = 0; i < 2; i++)
                    telosbdata[i] = (UInt16)((UInt16)(((UInt16)Buf[2 * i + 5]) << 8) + (UInt16)Buf[2 * i + 4]);
                telosbdata[0] = (UInt16)(telosbdata[0] + config);
                temperature = -39.4 + 0.01 * telosbdata[0];
                humidity = -2.0468 + 0.0367 * telosbdata[1] - 0.0000015955 * telosbdata[1] * telosbdata[1];
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
                        dewpoint = 272.62 * (Math.Log(humidity / 100.0) + 22.46 * temperature / (272.62 + temperature)) /
                            (22.46 - Math.Log(humidity / 100) - 22.46 * temperature / (272.62 + temperature));
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using System.Diagnostics;

namespace FlowrateConfig
{
    public partial class Form1 : Form
    {

        //ArrayList TempBoards = new ArrayList(100);
        ArrayList PumpBoards = new ArrayList(100);
        //ArrayList TelosbSensors = new ArrayList(200);
        //ArrayList Temperatureconfig = new ArrayList(100);
        ArrayList Flowrateconfig = new ArrayList(100);
        //ArrayList Telosbconfig = new ArrayList(100);

        //UInt16 CurTempNodID;
        UInt16 CurPumpNodeID;
        //UInt16 CurTelosbID;

        byte[] InBuf1 = new byte[64];

        Thread threadCom1Recv;
        bool threadCom1RecvEn;

        public Form1()
        {
            InitializeComponent();

            //CurTempNodID = 0;
            CurPumpNodeID = 0;
            //CurTelosbID = 0;

            ComSet comset = new ComSet(ref this.ComPort1);
            comset.ShowDialog();
            if (ComPort1.IsOpen == true)
                ComPort1.DiscardInBuffer();
            ComPort1.ReadTimeout = 50;

            //comboBoxT.Sorted = true;
            comboBoxP.Sorted = true;

            string curdirectory = Directory.GetCurrentDirectory();
            string newdirectory;
            newdirectory = curdirectory + "\\Config";
            if (Directory.Exists(newdirectory))
            {
                /*
                if (File.Exists(newdirectory + "\\Temperature.config") == true)
                {
                    ReadConfig(newdirectory + "\\Temperature.config", ref Temperatureconfig, 1);
                }
                */
                if (File.Exists(newdirectory + "\\Flowrate.config") == true)
                {
                    ReadConfig(newdirectory + "\\Flowrate.config", ref Flowrateconfig, 2);
                }
                /*
                if (File.Exists(newdirectory + "\\Telosb.config") == true)
                {
                    ReadConfig(newdirectory + "\\Telosb.config", ref Telosbconfig, 3);
                }
                */
            }

            threadCom1RecvEn = true;
            threadCom1Recv = new Thread(ThreadCom1Recv);
            threadCom1Recv.Start();
        }

        private void ReadConfig(string dir, ref ArrayList array, int mode)
        {
            FileStream fs;
            StreamReader sr;
            var fileMode = FileMode.Open;
            fs = new FileStream(dir, fileMode, FileAccess.Read);
            sr = new StreamReader(fs, Encoding.Default);
            /*
            if (mode == 1)
            {
                try
                {
                    while (true)
                    {
                        string line = sr.ReadLine();
                        if (line.StartsWith("#"))
                            continue;
                        if (line.Length < 3)
                            continue;
                        string[] split1 = line.Split(':');
                        string[] split2 = split1[1].Split(',');
                        TemperatureConfig newconfig = new TemperatureConfig();
                        try
                        {
                            newconfig.id = UInt16.Parse(split1[0]);
                            for (int i = 0; i < 16; i++)
                            {
                                newconfig.config[i] = int.Parse(split2[i]);
                            }
                        }
                        catch
                        {
                        }
                        Temperatureconfig.Add(newconfig);
                    }
                }
                catch
                {
                }
            }
            else*/ if (mode == 2)
            {
                try
                {
                    while (true)
                    {
                        string line = sr.ReadLine();
                        if (line.StartsWith("#"))
                            continue;
                        if (line.Length < 3)
                            continue;
                        string[] split1 = line.Split(':');
                        string[] split2 = split1[1].Split(',');
                        FlowrateConfig newconfig = new FlowrateConfig();
                        try
                        {
                            newconfig.id = UInt16.Parse(split1[0]);
                            for (int i = 0; i < 8; i++)
                            {
                                newconfig.config[i] = int.Parse(split2[i]);
                            }
                        }
                        catch
                        {
                        }
                        Flowrateconfig.Add(newconfig);
                    }
                }
                catch
                {
                }
            }
            /*
            else if (mode == 3)
            {
                try
                {
                    while (true)
                    {
                        string line = sr.ReadLine();
                        if (line.StartsWith("#"))
                            continue;
                        if (line.Length < 3)
                            continue;
                        string[] split1 = line.Split(':');
                        TelosbConfig newconfig = new TelosbConfig();
                        try
                        {
                            newconfig.id = UInt16.Parse(split1[0]);
                            newconfig.config = int.Parse(split1[1]);
                        }
                        catch
                        {
                        }
                        Telosbconfig.Add(newconfig);
                    }
                }
                catch
                {
                }
            }
            */
            else
            {
            }

            sr.Close();
            fs.Close();
        }

        private void ThreadCom1Recv()
        {
            byte temp;
            UInt16 len = 0;
            UInt16 Type;
            byte[] SerialRxBuf = new byte[64];
            int SerialCount = 0;
            byte RxSum = 0;

            MethodInvoker mi = new MethodInvoker(Com1InvokeFun);
            while (threadCom1RecvEn == true && ComPort1.IsOpen == true)
            {
                try
                {
                    temp = (byte)ComPort1.ReadByte();

                    if (SerialCount == 0)
                    {
                        if (temp == 0xAA)
                        {
                            SerialRxBuf[SerialCount] = temp;
                            SerialCount++;
                            RxSum = 0xAA;
                        }
                    }
                    else if (SerialCount == 1)
                    {
                        if (temp == 0x55)
                        {
                            SerialRxBuf[SerialCount] = temp;
                            SerialCount++;
                            RxSum += temp;
                        }
                        else if (temp != 0xAA)
                        {
                            SerialCount = 0;
                        }
                    }
                    else if (SerialCount == 2)
                    {
                        len = (UInt16)temp;
                        if (len > 60)
                            SerialCount = 0;
                        else
                        {
                            SerialRxBuf[SerialCount] = temp;
                            RxSum += temp;
                            SerialCount++;
                        }
                    }
                    else if (SerialCount < len)
                    {
                        SerialRxBuf[SerialCount] = temp;
                        RxSum += temp;
                        SerialCount++;

                        if (SerialCount == len)
                        {
                            SerialCount = 0;
                            if (RxSum == 0x00)
                            {
                                Type = (UInt16)SerialRxBuf[6];
                                Type += (UInt16)(((UInt16)SerialRxBuf[7]) << 8);
                                //if (Type == 0x0000 || Type == 0x0001 || Type == 0x0100 || Type == 0x0200 || Type == 0x0300 || Type == 0x0400 || Type == 0x0401)
                                if (Type == 0x0200)
                                {
                                    for (int i = 4; i < len; i++)
                                    {
                                        InBuf1[i - 4] = SerialRxBuf[i];
                                    }
                                    BeginInvoke(mi);
                                }
                            }
                        }
                    }
                    else
                    {
                        SerialCount = 0;
                        if (temp == 0xAA)
                        {
                            SerialRxBuf[SerialCount] = temp;
                            SerialCount++;
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void Com1InvokeFun()
        {
            UInt16 nodeID;
            UInt16 type;

            nodeID = (UInt16)InBuf1[0];
            nodeID += (UInt16)(((UInt16)InBuf1[1]) << 8);

            type = (UInt16)InBuf1[2];
            type += (UInt16)(((UInt16)InBuf1[3]) << 8);

            Com1ProcessData(nodeID, type);
            Display(nodeID, type);
        }

        private void Com1ProcessData(UInt16 nodeID, UInt16 type)
        {
            int i = 0;

            switch (type)
            {
                /*
                case 0x0000:
                case 0x0001:
                    if (TempBoards.Contains(nodeID) == false)
                    {
                        if (Temperatureconfig.Contains(nodeID) == true)
                        {
                            int index = Temperatureconfig.IndexOf(nodeID);
                            TempBoards.Add(new TemperatueBoard(nodeID, ((TemperatureConfig)Temperatureconfig[index])));
                        }
                        else
                            TempBoards.Add(new TemperatueBoard(nodeID));
                        comboBoxT.Items.Add(nodeID);

                        if (CurTempNodID == 0)
                        {
                            CurTempNodID = nodeID;
                            comboBoxT.SelectedItem = nodeID;
                        }
                    }
                    i = TempBoards.IndexOf(nodeID);
                    ((TemperatueBoard)TempBoards[i]).SetTemperature(InBuf1, (type & 0x01));
                    ((TemperatueBoard)TempBoards[i]).Port = 1;
                    ((TemperatueBoard)TempBoards[i]).Online_T = true;
                    break;
                 */
                 case 0x0100:
                    if (PumpBoards.Contains(nodeID) == false)
                    {
                        if (Flowrateconfig.Contains(nodeID) == true)
                        {
                            int index = Flowrateconfig.IndexOf(nodeID);
                            PumpBoards.Add(new PumpBoard(nodeID, ((FlowrateConfig)Flowrateconfig[index])));
                        }
                        else
                            PumpBoards.Add(new PumpBoard(nodeID));
                        comboBoxP.Items.Add(nodeID);
                        if (CurPumpNodeID == 0)
                        {
                            CurPumpNodeID = nodeID;
                            comboBoxP.SelectedItem = nodeID;
                        }
                    }
                    i = PumpBoards.IndexOf(nodeID);
                    ((PumpBoard)PumpBoards[i]).SetFlowrate(InBuf1);
                    ((PumpBoard)PumpBoards[i]).Port = 1;
                    ((PumpBoard)PumpBoards[i]).Online_T = true;
                    break;
                
                /*
                case 0x0200:
                    if (Airboxes.Contains(nodeID) == false)
                    {
                        Airboxes.Add(new Airbox(nodeID));
                    }
                    i = Airboxes.IndexOf(nodeID);
                    ((Airbox)Airboxes[i]).SetAirboxdata(InBuf1);
                    ((Airbox)Airboxes[i]).Port = 1;
                    ((Airbox)Airboxes[i]).Online_T = true;
                    break;
                case 0x0300:
                    if (CO2flaps.Contains(nodeID) == false)
                    {
                        CO2flaps.Add(new CO2flap(nodeID));
                    }
                    i = CO2flaps.IndexOf(nodeID);
                    ((CO2flap)CO2flaps[i]).SetCO2flapdata(InBuf1);
                    ((CO2flap)CO2flaps[i]).Port = 1;
                    ((CO2flap)CO2flaps[i]).Online_T = true;
                    break;
                case 0x0400:
                    if (TelosbSensors.Contains(nodeID) == false)
                    {
                        if (Telosbconfig.Contains(nodeID) == true)
                        {
                            int index = Telosbconfig.IndexOf(nodeID);
                            TelosbSensors.Add(new TelosbSensor(nodeID, ((TelosbConfig)Telosbconfig[index])));
                        }
                        else
                            TelosbSensors.Add(new TelosbSensor(nodeID));
                        listBoxTBID.Items.Add(nodeID);
                        listBoxTBTemperature.Items.Add((0.0).ToString());
                        listBoxTBHumidity.Items.Add((0.0).ToString());
                        listBoxTBDewPoint.Items.Add((0.0).ToString());
                    }
                    i = TelosbSensors.IndexOf(nodeID);
                    ((TelosbSensor)TelosbSensors[i]).SetTelosbdata(InBuf1);
                    ((TelosbSensor)TelosbSensors[i]).Port = 1;
                    ((TelosbSensor)TelosbSensors[i]).Online_T = true;
                    break;
                case 0x0401:
                    switch ((int)nodeID)
                    {
                        case 25:
                            if (AirboxSht75s.Contains((UInt16)1) == false)
                            {
                                AirboxSht75s.Add(new TelosbSensor((UInt16)1));
                                AirboxSht75s.Add(new TelosbSensor((UInt16)2));
                            }
                            i = AirboxSht75s.IndexOf((UInt16)1);
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf1);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 1;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            i = AirboxSht75s.IndexOf((UInt16)2);
                            for (int m = 0; m < 4; m++) InBuf1[m + 4] = InBuf1[m + 8];
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf1);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 1;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            break;
                        case 26:
                            if (AirboxSht75s.Contains((UInt16)3) == false)
                            {
                                AirboxSht75s.Add(new TelosbSensor((UInt16)3));
                                AirboxSht75s.Add(new TelosbSensor((UInt16)4));
                            }
                            i = AirboxSht75s.IndexOf((UInt16)3);
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf1);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 1;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            i = AirboxSht75s.IndexOf((UInt16)4);
                            for (int m = 0; m < 4; m++) InBuf1[m + 4] = InBuf1[m + 8];
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf1);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 1;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            break;
                        case 27:
                            if (AirboxSht75s.Contains((UInt16)5) == false)
                            {
                                AirboxSht75s.Add(new TelosbSensor((UInt16)5));
                                AirboxSht75s.Add(new TelosbSensor((UInt16)6));
                            }
                            i = AirboxSht75s.IndexOf((UInt16)5);
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf1);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 1;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            i = AirboxSht75s.IndexOf((UInt16)6);
                            for (int m = 0; m < 4; m++) InBuf1[m + 4] = InBuf1[m + 8];
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf1);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 1;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            break;
                        case 28:
                            if (AirboxSht75s.Contains((UInt16)7) == false)
                            {
                                AirboxSht75s.Add(new TelosbSensor((UInt16)7));
                                AirboxSht75s.Add(new TelosbSensor((UInt16)8));
                            }
                            i = AirboxSht75s.IndexOf((UInt16)7);
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf1);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 1;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            i = AirboxSht75s.IndexOf((UInt16)8);
                            for (int m = 0; m < 4; m++) InBuf1[m + 4] = InBuf1[m + 8];
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf1);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 1;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            break;
                        default:
                            break;

                    }
                    break;
                */
                default:
                    return;
            }

        }

        private void Display(UInt16 nodeID, UInt16 type)
        {
            int i = 0;
            switch (type)
            {
                /*
                case 0x0001:
                    if (nodeID == CurTempNodID)
                    {
                        i = TempBoards.IndexOf(nodeID);
                        if (i >= 0)
                        {
                            UInt16[] temperature = ((TemperatueBoard)TempBoards[i]).GetTemperature();
                            double tmp;
                            if (temperature[0] == 0x4000)
                            {
                                textBoxTCH1.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[0] / 16.0;
                                textBoxTCH1.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[1] == 0x4000)
                            {
                                textBoxTCH2.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[1] / 16.0;
                                textBoxTCH2.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[2] == 0x4000)
                            {
                                textBoxTCH3.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[2] / 16.0;
                                textBoxTCH3.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[3] == 0x4000)
                            {
                                textBoxTCH4.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[3] / 16.0;
                                textBoxTCH4.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[4] == 0x4000)
                            {
                                textBoxTCH5.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[4] / 16.0;
                                textBoxTCH5.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[5] == 0x4000)
                            {
                                textBoxTCH6.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[5] / 16.0;
                                textBoxTCH6.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[6] == 0x4000)
                            {
                                textBoxTCH7.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[6] / 16.0;
                                textBoxTCH7.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[7] == 0x4000)
                            {
                                textBoxTCH8.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[7] / 16.0;
                                textBoxTCH8.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[8] == 0x4000)
                            {
                                textBoxTCH9.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[8] / 16.0;
                                textBoxTCH9.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[9] == 0x4000)
                            {
                                textBoxTCH10.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[9] / 16.0;
                                textBoxTCH10.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[10] == 0x4000)
                            {
                                textBoxTCH11.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[10] / 16.0;
                                textBoxTCH11.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[11] == 0x4000)
                            {
                                textBoxTCH12.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[11] / 16.0;
                                textBoxTCH12.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[12] == 0x4000)
                            {
                                textBoxTCH13.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[12] / 16.0;
                                textBoxTCH13.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[13] == 0x4000)
                            {
                                textBoxTCH14.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[13] / 16.0;
                                textBoxTCH14.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[14] == 0x4000)
                            {
                                textBoxTCH15.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[14] / 16.0;
                                textBoxTCH15.Text = string.Format("{0:0.00}", tmp);
                            }

                            if (temperature[15] == 0x4000)
                            {
                                textBoxTCH16.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[15] / 16.0;
                                textBoxTCH16.Text = string.Format("{0:0.00}", tmp);
                            }
                        }
                    }
                    break;
                */
                case 0x0100:
                    if (nodeID == CurPumpNodeID)
                    {
                        i = PumpBoards.IndexOf(nodeID);
                        if (i >= 0)
                        {
                            double[] flowrate = ((PumpBoard)PumpBoards[i]).GetFlowrate();
                            textBoxFCH1.Text = string.Format("{0:0.000}", flowrate[0]);
                            textBoxFCH2.Text = string.Format("{0:0.000}", flowrate[1]);
                            textBoxFCH3.Text = string.Format("{0:0.000}", flowrate[2]);
                            textBoxFCH4.Text = string.Format("{0:0.000}", flowrate[3]);
                            textBoxFCH5.Text = string.Format("{0:0.000}", flowrate[4]);
                            textBoxFCH6.Text = string.Format("{0:0.000}", flowrate[5]);
                            textBoxFCH7.Text = string.Format("{0:0.000}", flowrate[6]);
                            textBoxFCH8.Text = string.Format("{0:0.000}", flowrate[7]);
                        }
                    }
                    break;
                /*
                case 0x0200:
                    if (nodeID >= 21 && nodeID <= 24)
                    {
                        i = Airboxes.IndexOf(nodeID);
                        if (i >= 0)
                        {
                            byte[] airboxdata = ((Airbox)Airboxes[i]).GetAirboxdata();
                            switch (nodeID)
                            {
                                case 21:
                                    textBoxAB1Status.Text = AirboxStatusExplain(airboxdata[0]);
                                    labelAB1Temp.Text = string.Format("Temperature: {0:0.0}°C", (airboxdata[3] * 0.5));
                                    labelAB1Fan.Text = string.Format("Fan Speed: {0}Hz", (airboxdata[4] * 50));
                                    labelAB1Power.Text = string.Format("Consuming Power: {0}W", (airboxdata[5] * 0.2));
                                    break;
                                case 22:
                                    textBoxAB2Status.Text = AirboxStatusExplain(airboxdata[0]);
                                    labelAB2Temp.Text = string.Format("Temperature: {0:0.0}°C", (airboxdata[3] * 0.5));
                                    labelAB2Fan.Text = string.Format("Fan Speed: {0}Hz", (airboxdata[4] * 50));
                                    labelAB2Power.Text = string.Format("Consuming Power: {0}W", (airboxdata[5] * 0.2));
                                    break;
                                case 23:
                                    textBoxAB3Status.Text = AirboxStatusExplain(airboxdata[0]);
                                    labelAB3Temp.Text = string.Format("Temperature: {0:0.0}°C", (airboxdata[3] * 0.5));
                                    labelAB3Fan.Text = string.Format("Fan Speed: {0}Hz", (airboxdata[4] * 50));
                                    labelAB3Power.Text = string.Format("Consuming Power: {0}W", (airboxdata[5] * 0.2));
                                    break;
                                case 24:
                                    textBoxAB4Status.Text = AirboxStatusExplain(airboxdata[0]);
                                    labelAB4Temp.Text = string.Format("Temperature: {0:0.0}°C", (airboxdata[3] * 0.5));
                                    labelAB4Fan.Text = string.Format("Fan Speed: {0}Hz", (airboxdata[4] * 50));
                                    labelAB4Power.Text = string.Format("Consuming Power: {0}W", (airboxdata[5] * 0.2));
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
                case 0x0300:
                    if (nodeID >= 31 && nodeID <= 34)
                    {
                        i = CO2flaps.IndexOf(nodeID);
                        if (i >= 0)
                        {
                            byte[] CO2flapdata = ((CO2flap)CO2flaps[i]).GetCO2flapdata();
                            switch (nodeID)
                            {
                                case 31:
                                    labelFP1Status.Text = CO2flapStatusExplain(CO2flapdata[0]);
                                    labelFP1Temperature.Text = "Temperature: " + (CO2flapdata[8] * 0.2).ToString() + "°C";
                                    labelFP1CO2.Text = "CO2 conentration: " + (CO2flapdata[7] * 10).ToString() + "ppm";
                                    labelFP1PPM_THRES.Text = "PPM_THRES: " + (CO2flapdata[2] * 10).ToString();
                                    labelFP1PPM_HYST.Text = "PPM_HYST: " + (CO2flapdata[3] * 10).ToString();
                                    labelFP1TEMP_THRES.Text = "TEMP_THRES: " + (CO2flapdata[4] * 0.2).ToString();
                                    labelFP1TEMP_HYST.Text = "TEMP_HYST: " + (CO2flapdata[5] * 0.2).ToString();
                                    if ((CO2flapdata[0] & 0x20) == 0x20)
                                        buttonFP1.Text = "Close flap";
                                    else
                                        buttonFP1.Text = "Open flap";
                                    break;
                                case 32:
                                    labelFP2Status.Text = CO2flapStatusExplain(CO2flapdata[0]);
                                    labelFP2Temperature.Text = "Temperature: " + (CO2flapdata[8] * 0.2).ToString() + "°C";
                                    labelFP2CO2.Text = "CO2 conentration: " + (CO2flapdata[7] * 10).ToString() + "ppm";
                                    labelFP2PPM_THRES.Text = "PPM_THRES: " + (CO2flapdata[2] * 10).ToString();
                                    labelFP2PPM_HYST.Text = "PPM_HYST: " + (CO2flapdata[3] * 10).ToString();
                                    labelFP2TEMP_THRES.Text = "TEMP_THRES: " + (CO2flapdata[4] * 0.2).ToString();
                                    labelFP2TEMP_HYST.Text = "TEMP_HYST: " + (CO2flapdata[5] * 0.2).ToString();
                                    if ((CO2flapdata[0] & 0x20) == 0x20)
                                        buttonFP2.Text = "Close flap";
                                    else
                                        buttonFP2.Text = "Open flap";
                                    break;
                                case 33:
                                    labelFP3Status.Text = CO2flapStatusExplain(CO2flapdata[0]);
                                    labelFP3Temperature.Text = "Temperature: " + (CO2flapdata[8] * 0.2).ToString() + "°C";
                                    labelFP3CO2.Text = "CO2 conentration: " + (CO2flapdata[7] * 10).ToString() + "ppm";
                                    labelFP3PPM_THRES.Text = "PPM_THRES: " + (CO2flapdata[2] * 10).ToString();
                                    labelFP3PPM_HYST.Text = "PPM_HYST: " + (CO2flapdata[3] * 10).ToString();
                                    labelFP3TEMP_THRES.Text = "TEMP_THRES: " + (CO2flapdata[4] * 0.2).ToString();
                                    labelFP3TEMP_HYST.Text = "TEMP_HYST: " + (CO2flapdata[5] * 0.2).ToString();
                                    if ((CO2flapdata[0] & 0x20) == 0x20)
                                        buttonFP3.Text = "Close flap";
                                    else
                                        buttonFP3.Text = "Open flap";
                                    break;
                                case 34:
                                    labelFP4Status.Text = CO2flapStatusExplain(CO2flapdata[0]);
                                    labelFP4Temperature.Text = "Temperature: " + (CO2flapdata[8] * 0.2).ToString() + "°C";
                                    labelFP4CO2.Text = "CO2 conentration: " + (CO2flapdata[7] * 10).ToString() + "ppm";
                                    labelFP4PPM_THRES.Text = "PPM_THRES: " + (CO2flapdata[2] * 10).ToString();
                                    labelFP4PPM_HYST.Text = "PPM_HYST: " + (CO2flapdata[3] * 10).ToString();
                                    labelFP4TEMP_THRES.Text = "TEMP_THRES: " + (CO2flapdata[4] * 0.2).ToString();
                                    labelFP4TEMP_HYST.Text = "TEMP_HYST: " + (CO2flapdata[5] * 0.2).ToString();
                                    if ((CO2flapdata[0] & 0x20) == 0x20)
                                        buttonFP4.Text = "Close flap";
                                    else
                                        buttonFP4.Text = "Open flap";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
                case 0x0400:
                    i = listBoxTBID.Items.IndexOf(nodeID);
                    int j = TelosbSensors.IndexOf(nodeID);
                    listBoxTBTemperature.Items[i] = string.Format("{0:0.00}", ((TelosbSensor)TelosbSensors[j]).Temperature);
                    listBoxTBHumidity.Items[i] = string.Format("{0:0.00}", ((TelosbSensor)TelosbSensors[j]).Humidity);
                    listBoxTBDewPoint.Items[i] = string.Format("{0:0.00}", ((TelosbSensor)TelosbSensors[j]).DewPoint);
                    break;
                case 0x0401:
                    switch ((int)nodeID)
                    {
                        case 25:
                            i = AirboxSht75s.IndexOf((UInt16)1);
                            labelAB1InAir.Text = "Input Air:" + string.Format("{0:0.00}°C {1:0.00}%  {2:0.00}°C", ((TelosbSensor)AirboxSht75s[i]).Temperature, ((TelosbSensor)AirboxSht75s[i]).Humidity, ((TelosbSensor)AirboxSht75s[i]).DewPoint);
                            i = AirboxSht75s.IndexOf((UInt16)2);
                            labelAB1OutAir.Text = "Output Air:" + string.Format("{0:0.00}°C {1:0.00}%  {2:0.00}°C", ((TelosbSensor)AirboxSht75s[i]).Temperature, ((TelosbSensor)AirboxSht75s[i]).Humidity, ((TelosbSensor)AirboxSht75s[i]).DewPoint);
                            break;
                        case 26:
                            i = AirboxSht75s.IndexOf((UInt16)3);
                            labelAB2InAir.Text = "Input Air:" + string.Format("{0:0.00}°C {1:0.00}%  {2:0.00}°C", ((TelosbSensor)AirboxSht75s[i]).Temperature, ((TelosbSensor)AirboxSht75s[i]).Humidity, ((TelosbSensor)AirboxSht75s[i]).DewPoint);
                            i = AirboxSht75s.IndexOf((UInt16)4);
                            labelAB2OutAir.Text = "Output Air:" + string.Format("{0:0.00}°C {1:0.00}%  {2:0.00}°C", ((TelosbSensor)AirboxSht75s[i]).Temperature, ((TelosbSensor)AirboxSht75s[i]).Humidity, ((TelosbSensor)AirboxSht75s[i]).DewPoint);
                            break;
                        case 27:
                            i = AirboxSht75s.IndexOf((UInt16)5);
                            labelAB3InAir.Text = "Input Air:" + string.Format("{0:0.00}°C {1:0.00}%  {2:0.00}°C", ((TelosbSensor)AirboxSht75s[i]).Temperature, ((TelosbSensor)AirboxSht75s[i]).Humidity, ((TelosbSensor)AirboxSht75s[i]).DewPoint);
                            i = AirboxSht75s.IndexOf((UInt16)6);
                            labelAB3OutAir.Text = "Output Air:" + string.Format("{0:0.00}°C {1:0.00}%  {2:0.00}°C", ((TelosbSensor)AirboxSht75s[i]).Temperature, ((TelosbSensor)AirboxSht75s[i]).Humidity, ((TelosbSensor)AirboxSht75s[i]).DewPoint);
                            break;
                        case 28:
                            i = AirboxSht75s.IndexOf((UInt16)7);
                            labelAB4InAir.Text = "Input Air:" + string.Format("{0:0.00}°C {1:0.00}%  {2:0.00}°C", ((TelosbSensor)AirboxSht75s[i]).Temperature, ((TelosbSensor)AirboxSht75s[i]).Humidity, ((TelosbSensor)AirboxSht75s[i]).DewPoint);
                            i = AirboxSht75s.IndexOf((UInt16)8);
                            labelAB4OutAir.Text = "Output Air:" + string.Format("{0:0.00}°C {1:0.00}%  {2:0.00}°C", ((TelosbSensor)AirboxSht75s[i]).Temperature, ((TelosbSensor)AirboxSht75s[i]).Humidity, ((TelosbSensor)AirboxSht75s[i]).DewPoint);
                            break;
                        default:
                            break;
                    }
                    break;
                */
                default:
                    break;
            }
        }

        private void buttonPort1_Click(object sender, EventArgs e)
        {
            threadCom1RecvEn = false;           
            if (threadCom1Recv.IsAlive)
                threadCom1Recv.Join();
            ComSet comset = new ComSet(ref this.ComPort1);
            comset.ShowDialog();
            threadCom1RecvEn = true;
            threadCom1Recv = new Thread(ThreadCom1Recv);
            threadCom1Recv.Start();           
        }
       

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            threadCom1RecvEn = false;

            if (threadCom1Recv.IsAlive)
                threadCom1Recv.Join();            
        }

        private void comboBoxT_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurPumpNodeID = UInt16.Parse(comboBoxP.SelectedItem.ToString());
            int index = PumpBoards.IndexOf(CurPumpNodeID);
            int[] config = ((PumpBoard)PumpBoards[index]).GetConfig();
            textBoxCFG1.Text = config[0].ToString();
            textBoxCFG2.Text = config[1].ToString();
            textBoxCFG3.Text = config[2].ToString();
            textBoxCFG4.Text = config[3].ToString();
            textBoxCFG5.Text = config[4].ToString();
            textBoxCFG6.Text = config[5].ToString();
            textBoxCFG7.Text = config[6].ToString();
            textBoxCFG8.Text = config[7].ToString();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            FileStream fs;
            StreamWriter sw;
            var fileMode = FileMode.Create;
            string dir = Directory.GetCurrentDirectory() + "\\Config\\Flowrate.config";
            fs = new FileStream(dir, fileMode, FileAccess.Write);
            sw = new StreamWriter(fs, Encoding.Default);

            string content;

            content = "##this file is the configuration file for the flowrate boards. Each line is a configuration for one board.\r\n";
            content += "#Each line should follow the following format:\r\n";
            content += "##ID:CH1,CH2,CH3,CH4,CH5,CH6,CH7,CH8\r\n";
            content += "#the ID is the ID of the flowrate board. Right now we got three boards 11, 12 and 13. The CH1 to CH8 rep-\r\n";
            content += "##resent the sensor information connected to the chanel. It is the pulse/l inforamtion of the sensor.\r\n\r\n";

            foreach (object o in Flowrateconfig)
            {
                if (PumpBoards.Contains(((FlowrateConfig)o).id) == false)
                {
                    content += ((FlowrateConfig)o).id.ToString() + ":";
                    content += string.Format("{0},{1},{2},{3},", ((FlowrateConfig)o).config[0], ((FlowrateConfig)o).config[1], ((FlowrateConfig)o).config[2], ((FlowrateConfig)o).config[3]);
                    content += string.Format("{0},{1},{2},{3}", ((FlowrateConfig)o).config[4], ((FlowrateConfig)o).config[5], ((FlowrateConfig)o).config[6], ((FlowrateConfig)o).config[7]);
                    content += "\r\n";
                }
            }
            foreach (object o in PumpBoards)
            {
                content += ((PumpBoard)o).ID.ToString() + ":";
                int[] config = ((PumpBoard)o).GetConfig();
                content += string.Format("{0},{1},{2},{3},", config[0], config[1], config[2], config[3]);
                content += string.Format("{0},{1},{2},{3}", config[4], config[5], config[6], config[7]);
                content += "\r\n";
            }

            sw.Write(content);
            sw.Close();
            fs.Close();
        }

        private void textBoxCFG1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (PumpBoards.Contains(CurPumpNodeID) == true)
                {
                    try
                    {
                        int i = int.Parse(textBoxCFG1.Text);
                        if (i > 0)
                        {
                            int index = PumpBoards.IndexOf(CurPumpNodeID);
                            ((PumpBoard)PumpBoards[index]).SetConfig(1, i);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error: wrong input. Please input a positive integer!");
                        int index = PumpBoards.IndexOf(CurPumpNodeID);
                        int[] config = ((PumpBoard)PumpBoards[index]).GetConfig();
                        textBoxCFG1.Text = config[0].ToString();
                    }
                }
            }
        }


        private void textBoxCFG2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (PumpBoards.Contains(CurPumpNodeID) == true)
                {
                    try
                    {
                        int i = int.Parse(textBoxCFG2.Text);
                        if (i > 0)
                        {
                            int index = PumpBoards.IndexOf(CurPumpNodeID);
                            ((PumpBoard)PumpBoards[index]).SetConfig(2, i);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error: wrong input. Please input a positive integer!");
                        int index = PumpBoards.IndexOf(CurPumpNodeID);
                        int[] config = ((PumpBoard)PumpBoards[index]).GetConfig();
                        textBoxCFG1.Text = config[1].ToString();
                    }
                }
            }
        }

        private void textBoxCFG3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (PumpBoards.Contains(CurPumpNodeID) == true)
                {
                    try
                    {
                        int i = int.Parse(textBoxCFG3.Text);
                        if (i > 0)
                        {
                            int index = PumpBoards.IndexOf(CurPumpNodeID);
                            ((PumpBoard)PumpBoards[index]).SetConfig(3, i);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error: wrong input. Please input a positive integer!");
                        int index = PumpBoards.IndexOf(CurPumpNodeID);
                        int[] config = ((PumpBoard)PumpBoards[index]).GetConfig();
                        textBoxCFG1.Text = config[2].ToString();
                    }
                }
            }
        }

        private void textBoxCFG4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (PumpBoards.Contains(CurPumpNodeID) == true)
                {
                    try
                    {
                        int i = int.Parse(textBoxCFG4.Text);
                        if (i > 0)
                        {
                            int index = PumpBoards.IndexOf(CurPumpNodeID);
                            ((PumpBoard)PumpBoards[index]).SetConfig(4, i);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error: wrong input. Please input a positive integer!");
                        int index = PumpBoards.IndexOf(CurPumpNodeID);
                        int[] config = ((PumpBoard)PumpBoards[index]).GetConfig();
                        textBoxCFG1.Text = config[3].ToString();
                    }
                }
            }
        }

        private void textBoxCFG5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (PumpBoards.Contains(CurPumpNodeID) == true)
                {
                    try
                    {
                        int i = int.Parse(textBoxCFG5.Text);
                        if (i > 0)
                        {
                            int index = PumpBoards.IndexOf(CurPumpNodeID);
                            ((PumpBoard)PumpBoards[index]).SetConfig(5, i);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error: wrong input. Please input a positive integer!");
                        int index = PumpBoards.IndexOf(CurPumpNodeID);
                        int[] config = ((PumpBoard)PumpBoards[index]).GetConfig();
                        textBoxCFG1.Text = config[4].ToString();
                    }
                }
            }
        }

        private void textBoxCFG6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (PumpBoards.Contains(CurPumpNodeID) == true)
                {
                    try
                    {
                        int i = int.Parse(textBoxCFG6.Text);
                        if (i > 0)
                        {
                            int index = PumpBoards.IndexOf(CurPumpNodeID);
                            ((PumpBoard)PumpBoards[index]).SetConfig(6, i);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error: wrong input. Please input a positive integer!");
                        int index = PumpBoards.IndexOf(CurPumpNodeID);
                        int[] config = ((PumpBoard)PumpBoards[index]).GetConfig();
                        textBoxCFG1.Text = config[5].ToString();
                    }
                }
            }
        }

        private void textBoxCFG7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (PumpBoards.Contains(CurPumpNodeID) == true)
                {
                    try
                    {
                        int i = int.Parse(textBoxCFG7.Text);
                        if (i > 0)
                        {
                            int index = PumpBoards.IndexOf(CurPumpNodeID);
                            ((PumpBoard)PumpBoards[index]).SetConfig(7, i);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error: wrong input. Please input a positive integer!");
                        int index = PumpBoards.IndexOf(CurPumpNodeID);
                        int[] config = ((PumpBoard)PumpBoards[index]).GetConfig();
                        textBoxCFG1.Text = config[6].ToString();
                    }
                }
            }
        }

        private void textBoxCFG8_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (PumpBoards.Contains(CurPumpNodeID) == true)
                {
                    try
                    {
                        int i = int.Parse(textBoxCFG8.Text);
                        if (i > 0)
                        {
                            int index = PumpBoards.IndexOf(CurPumpNodeID);
                            ((PumpBoard)PumpBoards[index]).SetConfig(8, i);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error: wrong input. Please input a positive integer!");
                        int index = PumpBoards.IndexOf(CurPumpNodeID);
                        int[] config = ((PumpBoard)PumpBoards[index]).GetConfig();
                        textBoxCFG1.Text = config[7].ToString();
                    }
                }
            }
        }
    }
}

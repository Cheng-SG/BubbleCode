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

namespace BubbleDataCollection
{
    public partial class Form1 : Form
    {
        string SyncFolder;
        int SyncInterval = 10;
        int StoreIterval = 2;

        ArrayList TempBoards = new ArrayList(100);
        ArrayList PumpBoards = new ArrayList(100);
        ArrayList Airboxes = new ArrayList(10);
        ArrayList CO2flaps = new ArrayList(10);
        ArrayList TelosbSensors = new ArrayList(200);
        
        UInt16 CurTempNodID;
        UInt16 CurPumpNodeID;
        
        
        int  SerialCount;
        byte RxSum, TxSum;
        byte[] SerialRxBuf = new byte[64];
        byte[] SerialTxBuf = new byte[64];


        UInt16 nodeID;
        UInt16 type;
        byte[] InBuf = new byte[64];
        

        Thread threadReceive;
        bool threadReceiveEn;

        Thread threadSend;
        bool threadSendEn;

        Thread threadUpload;
        bool shouldUpload = false;        
        bool overwrite = false;
        string datastoredirectory;
        string uploadstoredirectory;

        public Form1()
        {
            InitializeComponent();

            SyncFolder = Environment.GetEnvironmentVariable("USERPROFILE");
            SyncFolder += "\\Dropbox\\LowEx-Dropbox\\Bubble Data";
            if (Directory.Exists(SyncFolder) == false) SyncFolder = "";
            else folderBrowserDialog1.SelectedPath = SyncFolder;

            CurTempNodID = 0;
            CurPumpNodeID = 0;            

            ComSet comset = new ComSet(ref this.ComPort);
            comset.ShowDialog();
            if (ComPort.IsOpen == true)
                ComPort.DiscardInBuffer();
            ComPort.ReadTimeout = 50;

            comboBoxT.Sorted = true;
            comboBoxP.Sorted = true;
            listBoxTBID.Sorted = true;

            string curdirectory = Directory.GetCurrentDirectory();
            string newdirectory;
            newdirectory = curdirectory + "\\Data";
            if (Directory.Exists(newdirectory) == false)
            {
                Directory.CreateDirectory(newdirectory);
            }
            datastoredirectory = curdirectory + "\\Data";
            newdirectory = curdirectory + "\\Upload";
            if (Directory.Exists(newdirectory) == false)
            {
                Directory.CreateDirectory(newdirectory);
            }
            newdirectory = curdirectory + "\\Upload\\Upload0";
            if (Directory.Exists(newdirectory) == false)
            {
                Directory.CreateDirectory(newdirectory);
            }
            newdirectory = curdirectory + "\\Upload\\Upload1";
            if (Directory.Exists(newdirectory) == false)
            {
                Directory.CreateDirectory(newdirectory);
            }
            uploadstoredirectory = curdirectory + "\\Upload\\Upload0";

            textBoxFP1PPM_THRES.Text = "900";
            textBoxFP1PPM_HYST.Text = "300";
            textBoxFP1TEMP_THRES.Text = "30";
            textBoxFP1TEMP_HYST.Text = "2";
            textBoxFP2PPM_THRES.Text = "900";
            textBoxFP2PPM_HYST.Text = "300";
            textBoxFP2TEMP_THRES.Text = "30";
            textBoxFP2TEMP_HYST.Text = "2";
            textBoxFP3PPM_THRES.Text = "900";
            textBoxFP3PPM_HYST.Text = "300";
            textBoxFP3TEMP_THRES.Text = "30";
            textBoxFP3TEMP_HYST.Text = "2";
            textBoxFP4PPM_THRES.Text = "900";
            textBoxFP4PPM_HYST.Text = "300";
            textBoxFP4TEMP_THRES.Text = "30";
            textBoxFP4TEMP_HYST.Text = "2";
           
            threadReceiveEn = true;
            threadReceive = new Thread(ThreadReceive);
            threadReceive.Start();

            threadSendEn = true;
            threadSend = new Thread(ThreadSend);
            threadSend.Start();            

            timer1.Interval = SyncInterval * 60 * 1000;
            timer1.Start();

            timer2.Interval = StoreIterval * 1000;
            timer2.Start();

            timer3.Interval = 5 * 1000;
            timer3.Start();
        }

        private void ThreadReceive()
        {
            byte temp;
            UInt16 len = 0;            
            UInt16 Type;
            MethodInvoker mi = new MethodInvoker(InvokeFun);
            while (threadReceiveEn == true && ComPort.IsOpen == true)
            {
                try
                {
                    temp = (byte)ComPort.ReadByte();

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
                        else if(temp != 0xAA)
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
                                if (Type == 0x0000 || Type == 0x0001 || Type == 0x0100 || Type == 0x0200 || Type == 0x0300 || Type == 0x0400)
                                {
                                    for (int i = 4; i < len; i++)
                                    {
                                        InBuf[i-4] = SerialRxBuf[i];
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

        private void ThreadSend()
        {
            while (threadSendEn == true && ComPort.IsOpen == true)
            {
                foreach (object o in PumpBoards)
                {
                    if (((PumpBoard)o).Online == true && ((PumpBoard)o).NewOutData == true)
                    {
                        PumpSend(o);
                        ((PumpBoard)o).NewOutData = false;
                        Thread.Sleep(20);
                    }
                }
                foreach (object o in Airboxes)
                {
                    if (((Airbox)o).Online == true && ((Airbox)o).NewOutData == true)
                    {
                        AirboxSend(o);
                        ((Airbox)o).NewOutData = false;
                        Thread.Sleep(20);
                    }
                }
                foreach (object o in CO2flaps)
                {
                    if (((CO2flap)o).Online == true && ((CO2flap)o).NewOutData == true)
                    {
                        CO2flapSend(o);
                        ((CO2flap)o).NewOutData = false;
                        Thread.Sleep(20);
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void ThreadUpload()
        {
            shouldUpload = false;  
            overwrite = true;
            string dictory = uploadstoredirectory;
            string content = "";
            if (uploadstoredirectory == (Directory.GetCurrentDirectory() + "\\Upload\\Upload0"))
            {
                dictory = "Upload\\Upload0";
                uploadstoredirectory = Directory.GetCurrentDirectory() + "\\Upload\\Upload1";
            }
            else
            {
                dictory = "Upload\\Upload1";
                uploadstoredirectory = Directory.GetCurrentDirectory() + "\\Upload\\Upload0";
            }


            foreach (object o in TempBoards)
            {
                content += "curl -F file=@" + dictory + "\\Temperature" + ((TemperatueBoard)o).ID.ToString() + ".txt  http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php \r\n";              
            }

            foreach (object o in PumpBoards)
            {
                content += "curl -F file=@" + dictory + "\\FlowRate" + ((PumpBoard)o).ID.ToString() + ".txt  http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php \r\n";
            }

            foreach (object o in Airboxes)
            {
                content += "curl -F file=@" + dictory + "\\Airbox" + ((Airbox)o).ID.ToString() + ".txt  http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php \r\n";   
            }

            foreach (object o in CO2flaps)
            {
                content += "curl -F file=@" + dictory + "\\CO2flap" + ((CO2flap)o).ID.ToString() + ".txt  http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php \r\n";    
            }
            foreach (object o in TelosbSensors)
            {
                content += "curl -F file=@" + dictory + "\\Telosb" + ((TelosbSensor)o).ID.ToString() + ".txt  http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php \r\n";                   
            }
            
            
            string filename = Directory.GetCurrentDirectory() + "\\upload.bat";
            var fileMode = FileMode.Create;
            FileStream fs = new FileStream(filename, fileMode, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            sw.Write(content);
            sw.Close();
            fs.Close();

            Process proc = new Process();
            proc.StartInfo.FileName = "upload.bat";
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            try
            {    
                if (proc.Start() == true)
                {
                    proc.WaitForExit( (SyncInterval*60-10)*1000 );
                    if (proc.HasExited == false)
                    {
                        proc.Kill();
                    }
                } 
            }                
            catch
            {
            }
                
             
        }

        private void PumpSend(object o)
        {
            int i;
            TxSum = 0x00;
            SerialTxBuf[0] = 0xAA;
            SerialTxBuf[1] = 0x55;
            SerialTxBuf[2] = 24;
            SerialTxBuf[3] = 0x00;
            SerialTxBuf[4] = (byte)((PumpBoard)o).ID;
            SerialTxBuf[5] = (byte)((((PumpBoard)o).ID) >> 8);
            SerialTxBuf[6] = 0x01;
            SerialTxBuf[7] = 0x01;
            UInt16[] pumpspeed = ((PumpBoard)o).GetPumpspeed();
            for (i = 0; i < 8; i++)
            {
                SerialTxBuf[2 * i + 8] = (byte)(pumpspeed[i]);
                SerialTxBuf[2 * i + 9] = (byte)(pumpspeed[i] >> 8);
            }
            for (i = 0; i < 24; i++) TxSum += SerialTxBuf[i];
            TxSum = (byte)((~TxSum) + 1);
            SerialTxBuf[3] = TxSum;
            if (ComPort.IsOpen == true) ComPort.Write(SerialTxBuf, 0, 24);
        }

        private void AirboxSend(object o)
        {
            int i;
            TxSum = 0x00;
            SerialTxBuf[0] = 0xAA;
            SerialTxBuf[1] = 0x55;
            SerialTxBuf[2] = 12;
            SerialTxBuf[3] = 0x00;
            SerialTxBuf[4] = (byte)((Airbox)o).ID;
            SerialTxBuf[5] = (byte)((((Airbox)o).ID) >> 8);
            SerialTxBuf[6] = 0x02;
            SerialTxBuf[7] = 0x02;            
            SerialTxBuf[8] = 0x01;
            if (((Airbox)o).Speed == 1)
                SerialTxBuf[9] = 0x02;
            else
                SerialTxBuf[9] = 0x04;
            SerialTxBuf[10] = 14;
            SerialTxBuf[11] = ((Airbox)o).Speed;
            for (i = 0; i < 12; i++) TxSum += SerialTxBuf[i];
            TxSum = (byte)((~TxSum) + 1);
            SerialTxBuf[3] = TxSum;
            if (ComPort.IsOpen == true) ComPort.Write(SerialTxBuf, 0, 12);
        }

        private void CO2flapSend(object o)
        {
            int i;
            TxSum = 0x00;
            SerialTxBuf[0] = 0xAA;
            SerialTxBuf[1] = 0x55;
            SerialTxBuf[2] = 18;
            SerialTxBuf[3] = 0x00;
            SerialTxBuf[4] = (byte)((CO2flap)o).ID;
            SerialTxBuf[5] = (byte)((((CO2flap)o).ID) >> 8);
            SerialTxBuf[6] = 0x05;
            SerialTxBuf[7] = 0x03;            
            byte[] CO2flapdata = ((CO2flap)o).GetCO2flapParam();
            for (i = 0; i < 10; i++)
                SerialTxBuf[i + 8] = CO2flapdata[i];           
            for (i = 0; i < 18; i++) TxSum += SerialTxBuf[i];
            TxSum = (byte)((~TxSum) + 1);
            SerialTxBuf[3] = TxSum;
            if (ComPort.IsOpen == true) ComPort.Write(SerialTxBuf, 0, 18);
        }

        private void InvokeFun()
        {
            nodeID = (UInt16)InBuf[0];
            nodeID += (UInt16)(((UInt16)InBuf[1]) << 8);

            type = (UInt16)InBuf[2];
            type += (UInt16)(((UInt16)InBuf[3]) << 8);

            ProcessData();
            Display();
        }

        private void ProcessData()
        {
            int i = 0;                    

            switch (type)
            {
                case 0x0000:
                case 0x0001:
                    if (TempBoards.Contains(nodeID) == false)
                    {
                        TempBoards.Add(new TemperatueBoard(nodeID));
                        comboBoxT.Items.Add(nodeID);
                        
                        if (CurTempNodID == 0)
                        {
                            CurTempNodID = nodeID;
                            comboBoxT.SelectedItem = nodeID;
                        }
                    }
                    i = TempBoards.IndexOf(nodeID);
                    ((TemperatueBoard)TempBoards[i]).SetTemperature(InBuf, (type & 0x01));
                    ((TemperatueBoard)TempBoards[i]).Online_T = true;
                    break;
                case 0x0100:
                    if ( PumpBoards.Contains(nodeID) == false)
                    {
                        PumpBoards.Add(new PumpBoard(nodeID));
                        comboBoxP.Items.Add(nodeID);
                        if (CurPumpNodeID == 0)
                        {
                            CurPumpNodeID = nodeID;
                            comboBoxP.SelectedItem = nodeID;
                        }
                    }
                    i = PumpBoards.IndexOf(nodeID);
                    ((PumpBoard)PumpBoards[i]).SetFlowrate(InBuf);
                    ((PumpBoard)PumpBoards[i]).Online_T = true;                    
                    break;
                case 0x0200:
                    if (Airboxes.Contains(nodeID) == false)
                    {
                        Airboxes.Add(new Airbox(nodeID));                        
                    }
                    i = Airboxes.IndexOf(nodeID);
                    ((Airbox)Airboxes[i]).SetAirboxdata(InBuf);
                    ((Airbox)Airboxes[i]).Online_T = true;
                    break;
                case 0x0300:
                    if (CO2flaps.Contains(nodeID) == false)
                    {
                        CO2flaps.Add(new CO2flap(nodeID));
                    }
                    i = CO2flaps.IndexOf(nodeID);
                    ((CO2flap)CO2flaps[i]).SetCO2flapdata(InBuf);
                    ((CO2flap)CO2flaps[i]).Online_T = true;
                    break;
                case 0x0400:                    
                    if (TelosbSensors.Contains(nodeID) == false)
                    {
                        TelosbSensors.Add(new TelosbSensor(nodeID));                        
                        listBoxTBID.Items.Add(nodeID);
                        listBoxTBTemperature.Items.Add((0.0).ToString());
                        listBoxTBHumidity.Items.Add((0.0).ToString());
                    }
                    i = TelosbSensors.IndexOf(nodeID);
                    ((TelosbSensor)TelosbSensors[i]).SetTelosbdata(InBuf);
                    ((TelosbSensor)TelosbSensors[i]).Online_T = true;
                    break;
                default:
                    return;
            }           
           
        }

        private void Display()
        {
            int i = 0;
            switch (type)
            {
                case 0x0001:
                    if (nodeID == CurTempNodID)
                    {
                        i = TempBoards.IndexOf(nodeID);
                        if (i >=0 )
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
                case 0x0100:
                    if (nodeID == CurPumpNodeID)
                    {
                        i = PumpBoards.IndexOf(nodeID);
                        if (i >= 0)
                        {
                            UInt16[] flowrate = ((PumpBoard)PumpBoards[i]).GetFlowrate();
                            textBoxFCH1.Text = string.Format("{0:0.000}", (flowrate[0] / 9.4));
                            textBoxFCH2.Text = string.Format("{0:0.000}", (flowrate[1] / 9.4));
                            textBoxFCH3.Text = string.Format("{0:0.000}", (flowrate[2] / 9.4));
                            textBoxFCH4.Text = string.Format("{0:0.000}", (flowrate[3] / 9.4));
                            textBoxFCH5.Text = string.Format("{0:0.000}", (flowrate[4] / 9.4));
                            textBoxFCH6.Text = string.Format("{0:0.000}", (flowrate[5] / 9.4));
                            textBoxFCH7.Text = string.Format("{0:0.000}", (flowrate[6] / 9.4));
                            textBoxFCH8.Text = string.Format("{0:0.000}", (flowrate[7] / 9.4));
                        }
                    }
                    break;
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
                                    labelFP1PPM_THRES.Text = "PPM_THRES: "+(CO2flapdata[2] * 10).ToString();
                                    labelFP1PPM_HYST.Text = "PPM_HYST: "+(CO2flapdata[3] * 10).ToString();
                                    labelFP1TEMP_THRES.Text = "TEMP_THRES: "+(CO2flapdata[4] * 0.2).ToString();
                                    labelFP1TEMP_HYST.Text = "TEMP_HYST: "+(CO2flapdata[5] * 0.2).ToString();
                                    if ((CO2flapdata[0] & 0x20) == 0x20)
                                        buttonFP1.Text = "Close flap";
                                    else
                                        buttonFP1.Text = "Open flap";
                                    break;
                                case 32:
                                    labelFP2Status.Text = CO2flapStatusExplain(CO2flapdata[0]);
                                    labelFP2Temperature.Text = "Temperature: " + (CO2flapdata[8] * 0.2).ToString() + "°C";
                                    labelFP2CO2.Text = "CO2 conentration: " + (CO2flapdata[7] * 10).ToString() + "ppm";
                                    labelFP2PPM_THRES.Text = "PPM_THRES: "+(CO2flapdata[2] * 10).ToString();
                                    labelFP2PPM_HYST.Text = "PPM_HYST: "+(CO2flapdata[3] * 10).ToString();
                                    labelFP2TEMP_THRES.Text = "TEMP_THRES: "+(CO2flapdata[4] * 0.2).ToString();
                                    labelFP2TEMP_HYST.Text = "TEMP_HYST: "+(CO2flapdata[5] * 0.2).ToString();
                                    if ((CO2flapdata[0] & 0x20) == 0x20)
                                        buttonFP2.Text = "Close flap";
                                    else
                                        buttonFP2.Text = "Open flap";
                                    break;
                                case 33:
                                    labelFP3Status.Text = CO2flapStatusExplain(CO2flapdata[0]);
                                    labelFP3Temperature.Text = "Temperature: " + (CO2flapdata[8] * 0.2).ToString() + "°C";
                                    labelFP3CO2.Text = "CO2 conentration: " + (CO2flapdata[7] * 10).ToString() + "ppm";
                                    labelFP3PPM_THRES.Text = "PPM_THRES: "+(CO2flapdata[2] * 10).ToString();
                                    labelFP3PPM_HYST.Text = "PPM_HYST: "+(CO2flapdata[3] * 10).ToString();
                                    labelFP3TEMP_THRES.Text = "TEMP_THRES: "+(CO2flapdata[4] * 0.2).ToString();
                                    labelFP3TEMP_HYST.Text = "TEMP_HYST: "+(CO2flapdata[5] * 0.2).ToString();
                                    if ((CO2flapdata[0] & 0x20) == 0x20)
                                        buttonFP3.Text = "Close flap";
                                    else
                                        buttonFP3.Text = "Open flap";
                                    break;
                                case 34:
                                    labelFP4Status.Text = CO2flapStatusExplain(CO2flapdata[0]);
                                    labelFP4Temperature.Text = "Temperature: " + (CO2flapdata[8] * 0.2).ToString() + "°C";
                                    labelFP4CO2.Text = "CO2 conentration: " + (CO2flapdata[7] * 10).ToString() + "ppm";
                                    labelFP4PPM_THRES.Text = "PPM_THRES: "+(CO2flapdata[2] * 10).ToString();
                                    labelFP4PPM_HYST.Text = "PPM_HYST: "+(CO2flapdata[3] * 10).ToString();
                                    labelFP4TEMP_THRES.Text = "TEMP_THRES: "+(CO2flapdata[4] * 0.2).ToString();
                                    labelFP4TEMP_HYST.Text = "TEMP_HYST: "+(CO2flapdata[5] * 0.2).ToString();
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
                    break;
                default:
                    break;
            }
        }

        

        private string AirboxStatusExplain(byte status)
        {
            string ret = "";
            if ((status & 0x01) == 0x01)
            {
                ret = "No Error";
                if ((status & 0x20) == 0x20)
                    ret += " & Frost warning";
            }
            else if ((status & 0x80) == 0x80)
            {
                ret = "Error: ";
                if ((status & 0x02) == 0x02)
                    ret += "Sensor Error ";
                if ((status & 0x04) == 0x04)
                    ret += "Fan Error ";
                if ((status & 0x08) == 0x08)
                    ret += "Flap Error ";
                if ((status & 0x10) == 0x10)
                    ret += "Frost Error ";
            }
            return ret;
        }

        private string CO2flapStatusExplain(byte status)
        {
            string ret = "Status: ";
            if ((status & 0x80) == 0x80)
                ret += "Error";
            else
            {
                if ((status & 0x04) == 0x04)
                    ret += "PPM exceed limit value ";
                else
                    ret += "PPM below limit value ";
                if ((status & 0x20) == 0x20)
                    ret += "flap open ";
                else
                    ret += "flap closed ";
            }
            return ret;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (SyncFolder != "")
            {
                string origin,dest;                
                try
                {
                    foreach (object o in TempBoards)
                    {                       
                        origin = Directory.GetCurrentDirectory() + "\\Data\\Temperature" + ((TemperatueBoard)o).ID.ToString() + ".txt";
                        dest = SyncFolder + "\\Temperature" + ((TemperatueBoard)o).ID.ToString() + ".txt";
                        File.Copy(origin, dest, true);                        
                    }
                    foreach (object o in PumpBoards)
                    {
                        origin = Directory.GetCurrentDirectory() + "\\Data\\FlowRate" + ((PumpBoard)o).ID.ToString() + ".txt";
                        dest = SyncFolder + "\\FlowRate" + ((PumpBoard)o).ID.ToString() + ".txt";
                        File.Copy(origin, dest, true);                        
                    }
                    foreach (object o in Airboxes)
                    {
                        origin = Directory.GetCurrentDirectory() + "\\Data\\Airbox" + ((Airbox)o).ID.ToString() + ".txt";
                        dest = SyncFolder + "\\Airbox" + ((Airbox)o).ID.ToString() + ".txt";
                        File.Copy(origin, dest, true);
                    }
                    foreach (object o in CO2flaps)
                    {
                        origin = Directory.GetCurrentDirectory() + "\\Data\\CO2flap" + ((CO2flap)o).ID.ToString() + ".txt";
                        dest = SyncFolder + "\\CO2flap" + ((CO2flap)o).ID.ToString() + ".txt";
                        File.Copy(origin, dest, true);
                    }
                    foreach (object o in TelosbSensors)
                    {
                        origin = Directory.GetCurrentDirectory() + "\\Data\\Telosb" + ((TelosbSensor)o).ID.ToString() + ".txt";
                        dest = SyncFolder + "\\Telosb" + ((TelosbSensor)o).ID.ToString() + ".txt";
                        File.Copy(origin, dest, true);
                    }
                }
                catch
                {
                }
            }

            shouldUpload = true;
            if(threadUpload != null && threadUpload.IsAlive == true )
            {
                threadUpload.Abort();
            }
        }

        private void buttonPort_Click(object sender, EventArgs e)
        {
            threadReceiveEn = false;
            threadSendEn = false;
            threadReceive.Join();
            threadSend.Join();
            ComSet comset = new ComSet(ref this.ComPort);
            comset.ShowDialog();
            threadReceiveEn = true;
            threadReceive = new Thread(ThreadReceive);
            threadReceive.Start();
            threadSendEn = true;
            threadSend = new Thread(ThreadSend);
            threadSend.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            threadReceiveEn = false;
            threadSendEn = false;
            threadReceive.Join();
            threadSend.Join();
        }

        private void comboBoxT_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurTempNodID = UInt16.Parse(comboBoxT.SelectedItem.ToString());            
        }

        private void comboBoxP_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurPumpNodeID = UInt16.Parse(comboBoxP.SelectedItem.ToString());
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                int[] pumpvalue = ((PumpBoard)PumpBoards[i]).GetPumpvalue();
                trackBarCH1.Value = pumpvalue[0];
                trackBarCH2.Value = pumpvalue[1];
                trackBarCH3.Value = pumpvalue[2];
                trackBarCH4.Value = pumpvalue[3];
                trackBarCH5.Value = pumpvalue[4];
                trackBarCH6.Value = pumpvalue[5];
                trackBarCH7.Value = pumpvalue[6];
                trackBarCH8.Value = pumpvalue[7];                
            }
        }

       
        private void trackBarCH1_ValueChanged(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                if( (((PumpBoard)PumpBoards[i]).GetPumpvalue())[0] != trackBarCH1.Value )
                    ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH1.Value,1);                
            }
            labelPump1.Text = "Pump1:" + (trackBarCH1.Value * 10).ToString() + "%";
        }
        

        private void trackBarCH2_ValueChanged(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                if ((((PumpBoard)PumpBoards[i]).GetPumpvalue())[1] != trackBarCH2.Value)
                    ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH2.Value, 2);
            }
            labelPump2.Text = "Pump2:" + (trackBarCH2.Value * 10).ToString() + "%";
        }       

        private void trackBarCH3_ValueChanged(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                if ((((PumpBoard)PumpBoards[i]).GetPumpvalue())[2] != trackBarCH3.Value)
                    ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH3.Value, 3);
            }            
            labelPump3.Text = "Pump3:" + (trackBarCH3.Value * 10).ToString() + "%";
        }
        

        private void trackBarCH4_ValueChanged(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                if ((((PumpBoard)PumpBoards[i]).GetPumpvalue())[3] != trackBarCH4.Value)
                    ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH4.Value, 4);
            }  
            labelPump4.Text = "Pump4:" + (trackBarCH4.Value * 10).ToString() + "%";
        }
        

        private void trackBarCH5_ValueChanged(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                if ((((PumpBoard)PumpBoards[i]).GetPumpvalue())[4] != trackBarCH5.Value)
                    ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH5.Value, 5);
            }  
            labelPump5.Text = "Pump5:" + (trackBarCH5.Value * 10).ToString() + "%";
        }

        private void trackBarCH6_ValueChanged(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                if ((((PumpBoard)PumpBoards[i]).GetPumpvalue())[5] != trackBarCH6.Value)
                    ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH6.Value, 6);
            }  
            labelPump6.Text = "Pump6:" + (trackBarCH6.Value * 10).ToString() + "%";
        }

        private void trackBarCH7_ValueChanged(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                if ((((PumpBoard)PumpBoards[i]).GetPumpvalue())[6] != trackBarCH7.Value)
                    ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH7.Value, 7);
            }  
            labelPump7.Text = "Pump7:" + (trackBarCH7.Value * 10).ToString() + "%";
        }
        

        private void trackBarCH8_ValueChanged(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                if ((((PumpBoard)PumpBoards[i]).GetPumpvalue())[7] != trackBarCH8.Value)
                    ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH8.Value, 8);
            }  
            labelPump8.Text = "Pump8:" + (trackBarCH8.Value * 10).ToString() + "%";
        }        

        private void timer2_Tick(object sender, EventArgs e)
        {
            foreach (object o in TempBoards)
            {
                FileSave(0, o);
            }
            foreach (object o in PumpBoards)
            {
                FileSave(1, o);
            }
            foreach (object o in Airboxes)
            {
                FileSave(2, o);
            }
            foreach (object o in CO2flaps)
            {
                FileSave(3, o);
            }
            foreach (object o in TelosbSensors)
            {
                FileSave(4, o);
            }

            if (shouldUpload == true)
            {                
                threadUpload = new Thread(ThreadUpload);
                threadUpload.Start();
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            foreach (object o in TempBoards)
            {
                if (((TemperatueBoard)o).Online_T == true)
                    ((TemperatueBoard)o).Online = true;
                else
                    ((TemperatueBoard)o).Online = false;
                ((TemperatueBoard)o).Online_T = false;
            }
            foreach (object o in PumpBoards)
            {
                if (((PumpBoard)o).Online_T == true)
                    ((PumpBoard)o).Online = true;
                else
                    ((PumpBoard)o).Online = false;
                ((PumpBoard)o).Online_T = false;
            }
            foreach (object o in Airboxes)
            {
                if (((Airbox)o).Online_T == true)
                    ((Airbox)o).Online = true;
                else
                {
                    ((Airbox)o).Online = false;
                    switch ( (int)(((Airbox)o).ID) )
                    {
                        case 21:
                            textBoxAB1Status.Text = "Off line";
                            break;
                        case 22:
                            textBoxAB2Status.Text = "Off line";
                            break;
                        case 23:
                            textBoxAB3Status.Text = "Off line";
                            break;
                        case 24:
                            textBoxAB3Status.Text = "Off line";
                            break;
                        default:
                            break;
                    }
                }
                ((Airbox)o).Online_T = false;
            }
            foreach (object o in CO2flaps)
            {
                if (((CO2flap)o).Online_T == true)
                    ((CO2flap)o).Online = true;
                else
                {
                    ((CO2flap)o).Online = false;
                    switch( (int)(((CO2flap)o).ID) )
                    {
                        case 31:
                            labelFP1Status.Text = "Status: Off line";
                            break;
                        case 32:
                            labelFP2Status.Text = "Status: Off line";
                            break;
                        case 33:
                            labelFP3Status.Text = "Status: Off line";
                            break;
                        case 34:
                            labelFP4Status.Text = "Status: Off line";
                            break;
                        default:
                            break;
                    }
                }
                ((CO2flap)o).Online_T = false;
            }
            foreach (object o in TelosbSensors)
            {
                if (((TelosbSensor)o).Online_T == true)
                    ((TelosbSensor)o).Online = true;
                else
                    ((TelosbSensor)o).Online = false;
                ((TelosbSensor)o).Online_T = false;
            }

        }

        private void FileSave(UInt16 type,object o)
        {
            string filename;
            FileStream fs;
            StreamWriter sw;
            var fileMode = FileMode.Append;
            string content;
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;

            if (overwrite == true)
            {
                foreach (object oo in TempBoards)
                {
                    ((TemperatueBoard)oo).UploadCreateNewFile = true;                    
                }
                foreach (object oo in PumpBoards)
                {
                    ((PumpBoard)oo).UploadCreateNewFile = true;                    
                }
                foreach (object oo in Airboxes)
                {
                    ((Airbox)oo).UploadCreateNewFile = true;                                     
                }
                foreach (object oo in CO2flaps)
                {
                    ((CO2flap)oo).UploadCreateNewFile = true;
                    
                }
                foreach (object oo in TelosbSensors)
                {
                    ((TelosbSensor)oo).UploadCreateNewFile = true;
                }
                overwrite = false;
            }
            
            if (type == 0)
            {
                if (((TemperatueBoard)o).Online == true && ((TemperatueBoard)o).NewInData == true)
                {                                        
                    content = string.Format("{0:yyyy-M-d;H:m:s}", currentTime);
                    UInt16[] temperature = ((TemperatueBoard)o).GetTemperature();
                    for (int i = 0; i < 16; i++)
                    {
                        content += ";";
                        if (temperature[i] == 0x4000)
                            content += "NC";
                        else
                            content += string.Format("{0:0.00}", (temperature[i] / 16.0) );
                    }
                    content += "\n";

                    filename = datastoredirectory + "\\Temperature" + ((TemperatueBoard)o).ID.ToString() + ".txt";
                    fileMode = File.Exists(filename) ? FileMode.Append : FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default);
                    sw.Write(content);
                    sw.Close();
                    fs.Close();

                    filename = uploadstoredirectory + "\\Temperature" + ((TemperatueBoard)o).ID.ToString() + ".txt";
                    fileMode = ((TemperatueBoard)o).UploadCreateNewFile ? FileMode.Create : FileMode.Append;
                    if (fileMode == FileMode.Append && File.Exists(filename) == false) fileMode = FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default);
                    sw.Write(content);
                    sw.Close();
                    fs.Close();
                    ((TemperatueBoard)o).UploadCreateNewFile = false;
                    
                    ((TemperatueBoard)o).NewInData = false;
                }
            }
            else if (type == 1)
            {         
                if (((PumpBoard)o).Online == true && ((PumpBoard)o).NewInData == true)
                {
                    content = string.Format("{0:yyyy-M-d;H:m:s}", currentTime);
                    UInt16[] flowrate = ((PumpBoard)o).GetFlowrate();
                    for (int i = 0; i < 8; i++)
                    {
                        content += ";";
                        content += string.Format( "{0:0.00}",(flowrate[i] / 9.4) );
                    }
                    content += "\n";

                    filename = datastoredirectory + "\\FlowRate" + ((PumpBoard)o).ID.ToString() + ".txt";
                    fileMode = File.Exists(filename) ? FileMode.Append : FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default);
                    sw.Write(content);
                    sw.Close();
                    fs.Close();

                    filename = uploadstoredirectory + "\\FlowRate" + ((PumpBoard)o).ID.ToString() + ".txt";
                    fileMode = ((PumpBoard)o).UploadCreateNewFile ? FileMode.Create : FileMode.Append;
                    if (fileMode == FileMode.Append && File.Exists(filename) == false) fileMode = FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default);                    
                    sw.Write(content);
                    sw.Close();
                    fs.Close();
                    ((PumpBoard)o).UploadCreateNewFile = false;

                    ((PumpBoard)o).NewInData = false;
                }
            }
            else if (type == 2)
            {
                if (((Airbox)o).Online == true && ((Airbox)o).NewInData == true)
                {               
                    byte[] airboxdata = ((Airbox)o).GetAirboxdata();
                    content = string.Format("{0:yyyy-M-d;H:m:s}", currentTime);
                    content += string.Format(";{0:0.0} °C", (airboxdata[3] * 0.5));
                    content += string.Format(";{0} Hz", (airboxdata[4] * 50) );
                    content += string.Format(";{0:0.0} W", (airboxdata[5] * 0.2).ToString() );
                    content += "\n";

                    filename = datastoredirectory + "\\Airbox" + ((Airbox)o).ID.ToString() + ".txt";
                    fileMode = File.Exists(filename) ? FileMode.Append : FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default); 
                    sw.Write(content);
                    sw.Close();
                    fs.Close();


                    filename = uploadstoredirectory + "\\Airbox" + ((Airbox)o).ID.ToString() + ".txt";
                    fileMode = ((Airbox)o).UploadCreateNewFile ? FileMode.Create : FileMode.Append;
                    if (fileMode == FileMode.Append && File.Exists(filename) == false) fileMode = FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default);                   
                    sw.Write(content);
                    sw.Close();
                    fs.Close();
                    ((Airbox)o).UploadCreateNewFile = false;

                    ((Airbox)o).NewInData = false;
                }       
            }
            else if (type == 3)
            {
                if (((CO2flap)o).Online == true && ((CO2flap)o).NewInData == true)
                {                                     
                    byte[] CO2flapdata = ((CO2flap)o).GetCO2flapdata();
                    content = string.Format("{0:yyyy-M-d;H:m:s}", currentTime);
                    content += string.Format(";{0:0.0} °C", (CO2flapdata[8] * 0.2));
                    content += string.Format(";{0} ppm", (CO2flapdata[7] * 10));
                    content += "\n";

                    filename = datastoredirectory + "\\CO2flap" + ((CO2flap)o).ID.ToString() + ".txt";
                    fileMode = File.Exists(filename) ? FileMode.Append : FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default); 
                    sw.Write(content);
                    sw.Close();
                    fs.Close();


                    filename = uploadstoredirectory + "\\CO2flap" + ((CO2flap)o).ID.ToString() + ".txt";
                    fileMode = ((CO2flap)o).UploadCreateNewFile ? FileMode.Create : FileMode.Append;
                    if (fileMode == FileMode.Append && File.Exists(filename) == false) fileMode = FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default);                                     
                    sw.Write(content);
                    sw.Close();
                    fs.Close();
                    ((CO2flap)o).UploadCreateNewFile = false;

                    ((CO2flap)o).NewInData = false;
                }
            }
            else if (type == 4)
            {
                if (((TelosbSensor)o).Online == true && ((TelosbSensor)o).NewInData == true)
                {                    
                    content = string.Format("{0:yyyy-M-d;H:m:s}", currentTime);
                    content += string.Format(";{0:0.00}", ((TelosbSensor)o).Temperature);
                    content += string.Format(";{0:0.00}", ((TelosbSensor)o).Humidity);
                    content += "\n";

                    filename = datastoredirectory + "\\Telosb" + ((TelosbSensor)o).ID.ToString() + ".txt";
                    fileMode = File.Exists(filename) ? FileMode.Append : FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default);
                    sw.Write(content);
                    sw.Close();
                    fs.Close();

                    filename = uploadstoredirectory + "\\Telosb" + ((TelosbSensor)o).ID.ToString() + ".txt";
                    fileMode = ((TelosbSensor)o).UploadCreateNewFile ? FileMode.Create : FileMode.Append;
                    if (fileMode == FileMode.Append && File.Exists(filename) == false) fileMode = FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default);
                    sw.Write(content);
                    sw.Close();
                    fs.Close();
                    ((TelosbSensor)o).UploadCreateNewFile = false;

                    ((TelosbSensor)o).NewInData = false;
                }
            }
        }

        private void textBoxSync_TextChanged(object sender, EventArgs e)
        {
            int i = 0;
            try
            {
                i = int.Parse(textBoxSync.Text);                
            }
            catch
            {
                if (textBoxSync.Text != "")
                {
                    MessageBox.Show("Wrong input");
                    textBoxSync.Text = SyncInterval.ToString();
                }
                return;
            }

            if(i<=0)
            {
                MessageBox.Show("Wrong input");                
                textBoxSync.Text = SyncInterval.ToString();
                return;
            }

            SyncInterval = i;
            timer1.Stop();
            timer1.Interval = i * 60 * 1000;
            timer1.Start();
        }

        private void textBoxStrInterval_TextChanged(object sender, EventArgs e)
        {
            int i = 0;
            try
            {
                i = int.Parse(textBoxStrInterval.Text);
            }
            catch
            {
                if (textBoxStrInterval.Text != "")
                {
                    MessageBox.Show("Wrong input");
                    textBoxStrInterval.Text = StoreIterval.ToString();
                }
                return;
            }

            if (i <= 0)
            {
                MessageBox.Show("Wrong input");                
                textBoxStrInterval.Text = StoreIterval.ToString();
                return;
            }

            StoreIterval = i;
            timer2.Stop();
            timer2.Interval = i * 1000;
            timer2.Start();
        }

        private void trackBarAB1_ValueChanged(object sender, EventArgs e)
        {
            int i = Airboxes.IndexOf((UInt16)21);
            if (i >= 0)
            {               
                ((Airbox)Airboxes[i]).SetAirboxvalue(trackBarAB1.Value);
            }
        }

        private void trackBarAB2_ValueChanged(object sender, EventArgs e)
        {
            int i = Airboxes.IndexOf((UInt16)22);
            if (i >= 0)
            {
                ((Airbox)Airboxes[i]).SetAirboxvalue(trackBarAB2.Value);
            }
        }

        private void trackBarAB3_ValueChanged(object sender, EventArgs e)
        {
            int i = Airboxes.IndexOf((UInt16)23);
            if (i >= 0)
            {
                ((Airbox)Airboxes[i]).SetAirboxvalue(trackBarAB3.Value);
            }
        }

        private void trackBarAB4_ValueChanged(object sender, EventArgs e)
        {
            int i = Airboxes.IndexOf((UInt16)24);
            if (i >= 0)
            {
                ((Airbox)Airboxes[i]).SetAirboxvalue(trackBarAB4.Value);
            }
        }

        private void buttonFP1_Click(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)31);
            if (i >= 0)
            {
                if (buttonFP1.Text == "Open flap")
                {
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam(3, 0);
                }
                else
                {
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam(2, 0);
                }
            }
        }

        private void textBoxFP1PPM_THRES_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)31);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP1PPM_THRES.Text);
                    j = j / 10;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 1);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP1PPM_HYST_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)31);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP1PPM_HYST.Text);
                    j = j / 10;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 2);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP1TEMP_THRES_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)31);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP1TEMP_THRES.Text);
                    j = j * 5;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 3);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP1TEMP_HYST_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)31);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP1TEMP_HYST.Text);
                    j = j * 5;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 4);
                }
                catch
                {
                }
            }
        }

        private void buttonFP2_Click(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)32);
            if (i >= 0)
            {
                if (buttonFP2.Text == "Open flap")
                {
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam(3, 0);
                }
                else
                {
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam(2, 0);
                }
            }
        }

        private void textBoxFP2PPM_THRES_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)32);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP2PPM_THRES.Text);
                    j = j / 10;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 1);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP2PPM_HYST_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)32);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP2PPM_HYST.Text);
                    j = j / 10;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 2);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP2TEMP_THRES_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)32);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP2TEMP_THRES.Text);
                    j = j * 5;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 3);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP2TEMP_HYST_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)32);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP2TEMP_HYST.Text);
                    j = j * 5;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 4);
                }
                catch
                {
                }
            }
        }

        private void buttonFP3_Click(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)33);
            if (i >= 0)
            {
                if (buttonFP3.Text == "Open flap")
                {
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam(3, 0);
                }
                else
                {
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam(2, 0);
                }
            }
        }

        private void textBoxFP3PPM_THRES_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)33);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP3PPM_THRES.Text);
                    j = j / 10;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 1);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP3PPM_HYST_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)33);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP3PPM_HYST.Text);
                    j = j / 10;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 2);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP3TEMP_THRES_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)33);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP3TEMP_THRES.Text);
                    j = j * 5;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 3);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP3TEMP_HYST_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)33);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP3TEMP_HYST.Text);
                    j = j * 5;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 4);
                }
                catch
                {
                }
            }
        }

        private void buttonFP4_Click(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)34);
            if (i >= 0)
            {
                if (buttonFP4.Text == "Open flap")
                {
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam(3, 0);
                }
                else
                {
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam(2, 0);
                }
            }
        }

        private void textBoxFP4PPM_THRES_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)34);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP4PPM_THRES.Text);
                    j = j / 10;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 1);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP4PPM_HYST_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)34);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP4PPM_HYST.Text);
                    j = j / 10;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 2);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP4TEMP_THRES_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)34);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP4TEMP_THRES.Text);
                    j = j * 5;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 3);
                }
                catch
                {
                }
            }
        }

        private void textBoxFP4TEMP_HYST_TextChanged(object sender, EventArgs e)
        {
            int i = CO2flaps.IndexOf((UInt16)34);
            if (i >= 0)
            {
                try
                {
                    int j = int.Parse(textBoxFP4TEMP_HYST.Text);
                    j = j * 5;
                    ((CO2flap)CO2flaps[i]).SetCO2flapParam((byte)j, 4);
                }
                catch
                {
                }
            }
        }
 
    }
}

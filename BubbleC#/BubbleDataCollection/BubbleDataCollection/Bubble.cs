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
        bool enSync;
        string SyncFolder;
        int SyncInterval = 12*60; //synchoronize interval in minutes;
        bool enUpload;
        int UploadInterval = 10;  //data upload interval in minutes;
        bool enStore;
        int StoreIterval = 10;    //data storage interval in seconds;

        ArrayList TempBoards = new ArrayList(100);
        ArrayList PumpBoards = new ArrayList(100);
        ArrayList Airboxes = new ArrayList(10);
        ArrayList AirboxSht75s = new ArrayList(10);
        ArrayList CO2flaps = new ArrayList(10);
        ArrayList TelosbSensors = new ArrayList(200);
        ArrayList Temperatureconfig = new ArrayList(100);
        ArrayList Flowrateconfig = new ArrayList(100);
        ArrayList Telosbconfig = new ArrayList(100);
        ArrayList Boardconfig = new ArrayList(100);

        UInt16 CurTempNodID;
        UInt16 CurPumpNodeID;

        byte[] InBuf1 = new byte[64];
        byte[] InBuf2 = new byte[64];

        Thread threadCom1Recv;
        bool threadCom1RecvEn;
        Thread threadCom2Recv;
        bool threadCom2RecvEn;

        Thread threadComSend;
        bool threadComSendEn;

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
            enSync = checkBoxSync.Checked;
            enUpload = checkBoxUpload.Checked;
            enStore = checkBoxStore.Checked;

            CurTempNodID = 0;
            CurPumpNodeID = 0;            

            ComSet comset = new ComSet(ref this.ComPort1);
            comset.ShowDialog();
            if (ComPort1.IsOpen == true)
                ComPort1.DiscardInBuffer();
            ComPort1.ReadTimeout = 50;

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
            newdirectory = curdirectory + "\\Config";
            if (Directory.Exists(newdirectory))
            {
                if (File.Exists(newdirectory + "\\Temperature.config") == true)
                {
                    ReadConfig(newdirectory + "\\Temperature.config",ref Temperatureconfig, 1);
                }
                if (File.Exists(newdirectory + "\\Flowrate.config") == true)
                {
                    ReadConfig(newdirectory + "\\Flowrate.config", ref Flowrateconfig, 2);
                }
                if (File.Exists(newdirectory + "\\Telosb.config") == true)
                {
                    ReadConfig(newdirectory + "\\Telosb.config", ref Telosbconfig, 3);
                }
                if (File.Exists(newdirectory + "\\Boards.config") == true)
                {
                    ReadConfig(newdirectory + "\\Boards.config", ref Boardconfig, 4);
                }
            }

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

            threadCom1RecvEn = true;
            threadCom1Recv = new Thread(ThreadCom1Recv);
            threadCom1Recv.Start();

            threadCom2RecvEn = false;
            threadCom2Recv = new Thread(ThreadCom2Recv);

            threadComSendEn = true;
            threadComSend = new Thread(ThreadComSend);
            threadComSend.Start();

            textBoxSync.Text = SyncInterval.ToString();
            timerSync.Interval = SyncInterval * 60 * 1000;
            timerSync.Start();

            textBoxStrInterval.Text = StoreIterval.ToString();
            timerStore.Interval = StoreIterval * 1000;
            timerStore.Start();

            textBoxUpload.Text = UploadInterval.ToString();
            timerUpload.Interval = UploadInterval * 60 * 1000;
            timerUpload.Start();

            timerCheckStatus.Interval = 5 * 1000;
            timerCheckStatus.Start();
        }

        private void ReadConfig(string dir, ref ArrayList array,int mode)
        {
            FileStream fs;
            StreamReader sr;
            var fileMode = FileMode.Open;
            fs = new FileStream(dir, fileMode, FileAccess.Read);
            sr = new StreamReader(fs, Encoding.Default);
            if (mode == 1)
            {
                try
                {
                    while (true)
                    {
                        string line = sr.ReadLine();
                        if(line.StartsWith("#"))
                            continue;
                        if(line.Length<3)
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
            else if (mode == 2)
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
            else if(mode ==3)
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
            else if (mode == 4)
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
                        if (split1.Length == 4)
                        {
                            UInt16 id;
                            try
                            {
                                id = UInt16.Parse(split1[0]);
                            }
                            catch
                            {
                                continue;
                            }

                            int type = -1;
                            if (split1[1] == "TEMPERATURE")
                                type = (int)SensorType.TEMPBOARD;
                            else if (split1[1] == "FLOWRATE" || split1[1] == "PUMP")
                                type = (int)SensorType.PUMPBOARD;
                            else
                                continue;

                            if (Boardconfig.Contains(id) == false)
                            {
                                BoardConfig newconfig = new BoardConfig(id, type);
                                Boardconfig.Add(newconfig);
                            }

                            int i = Boardconfig.IndexOf(id);
                            if(i>=0)
                                ((BoardConfig)Boardconfig[i]).SetConfig(split1);
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
            }
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
                                if (Type == 0x0000 || Type == 0x0001 || Type == 0x0100 || Type == 0x0200 || Type == 0x0300 || Type == 0x0400 || Type == 0x0401)
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

        private void ThreadCom2Recv()
        {
            byte temp;
            UInt16 len = 0;
            UInt16 Type;
            byte[] SerialRxBuf = new byte[64];
            int SerialCount = 0;
            byte RxSum = 0;

            MethodInvoker mi = new MethodInvoker(Com2InvokeFun);
            while (threadCom2RecvEn == true && ComPort2.IsOpen == true)
            {
                try
                {
                    temp = (byte)ComPort2.ReadByte();

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
                                if (Type == 0x0000 || Type == 0x0001 || Type == 0x0100 || Type == 0x0200 || Type == 0x0300 || Type == 0x0400 || Type == 0x0401)
                                {
                                    for (int i = 4; i < len; i++)
                                    {
                                        InBuf2[i - 4] = SerialRxBuf[i];
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

        private void ThreadComSend()
        {
            while (threadComSendEn == true)
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
                Thread.Sleep(20);
            }
        } 

        private void ThreadUpload()
        {
            int i = 0;
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

            if (TempBoards.Count > 0)
            {              
                foreach (object o in TempBoards)
                {
                    content += ":NEXT" + i + "\r\n" + "FOR %%A IN (1 2 3 4 5) DO (\r\n";
                    content += "curl --retry 5 -F file=@" + dictory + "\\Temperature" + ((TemperatueBoard)o).ID.ToString() + ".txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log\r\n";
                    content += "find \"failed\" upload.log\r\nIF ERRORLEVEL 1 GOTO NEXT"+ (i+1) +")\r\n\r\n";
                    i++;
                }
            }

            if (PumpBoards.Count > 0)
            {
                foreach (object o in PumpBoards)
                {
                    content += ":NEXT" + i + "\r\n" + "FOR %%A IN (1 2 3 4 5) DO (\r\n";
                    content += "curl --retry 5 -F file=@" + dictory + "\\FlowRate" + ((PumpBoard)o).ID.ToString() + ".txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log\r\n";
                    content += "find \"failed\" upload.log\r\nIF ERRORLEVEL 1 GOTO NEXT" + (i + 1) + ")\r\n\r\n";
                    i++;
                }
            }

            if (Airboxes.Count > 0)
            {
                foreach (object o in Airboxes)
                {
                    content += ":NEXT" + i + "\r\n" + "FOR %%A IN (1 2 3 4 5) DO (\r\n";
                    content += "curl --retry 5 -F file=@" + dictory + "\\Airbox" + ((Airbox)o).ID.ToString() + ".txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log\r\n";
                    content += "find \"failed\" upload.log\r\nIF ERRORLEVEL 1 GOTO NEXT" + (i + 1) + ")\r\n\r\n";
                    i++;
                }
            }

            if (AirboxSht75s.Count > 0)
            {
                foreach (object o in AirboxSht75s)
                {
                    content += ":NEXT" + i + "\r\n" + "FOR %%A IN (1 2 3 4 5) DO (\r\n";
                    content += "curl --retry 5 -F file=@" + dictory + "\\Sht75" + ((TelosbSensor)o).ID.ToString() + ".txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log\r\n";
                    content += "find \"failed\" upload.log\r\nIF ERRORLEVEL 1 GOTO NEXT" + (i + 1) + ")\r\n\r\n";
                    i++;
                }
            }

            if (CO2flaps.Count > 0)
            {
                foreach (object o in CO2flaps)
                {
                    content += ":NEXT" + i + "\r\n" + "FOR %%A IN (1 2 3 4 5) DO (\r\n";
                    content += "curl --retry 5 -F file=@" + dictory + "\\CO2flap" + ((CO2flap)o).ID.ToString() + ".txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log\r\n";
                    content += "find \"failed\" upload.log\r\nIF ERRORLEVEL 1 GOTO NEXT" + (i + 1) + ")\r\n\r\n";
                    i++;
                }
            }

            if (TelosbSensors.Count > 0)
            {
                foreach (object o in TelosbSensors)
                {
                    content += ":NEXT" + i + "\r\n" + "FOR %%A IN (1 2 3 4 5) DO (\r\n";
                    content += "curl --retry 5 -F file=@" + dictory + "\\Telosb" + ((TelosbSensor)o).ID.ToString() + ".txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log\r\n";
                    content += "find \"failed\" upload.log\r\nIF ERRORLEVEL 1 GOTO NEXT" + (i + 1) + ")\r\n\r\n";
                    i++;
                }
            }

            content += ":NEXT" + i + "\r\n";
            content += "DEL /F /Q upload.log";
            
            
            string filename = Directory.GetCurrentDirectory() + "\\upload.bat";
            var fileMode = FileMode.Create;
            FileStream fs = new FileStream(filename, fileMode, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            sw.Write(content);
            sw.Close();
            fs.Close();

            if (i > 0)
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "upload.bat";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                try
                {
                    if (proc.Start() == true)
                    {
                        proc.WaitForExit((SyncInterval * 60 - 10) * 1000);
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
                
             
        }

        private void PumpSend(object o)
        {
            int i;
            byte TxSum;
            byte[] SerialTxBuf = new byte[64];

            TxSum = 0x00;
            SerialTxBuf[0] = 0xAA;
            SerialTxBuf[1] = 0x55;
            SerialTxBuf[2] = 24;
            SerialTxBuf[3] = 0x00;
            SerialTxBuf[4] = (byte)((PumpBoard)o).ID;
            SerialTxBuf[5] = (byte)((((PumpBoard)o).ID) >> 8);
            SerialTxBuf[6] = 0x01;
            SerialTxBuf[7] = 0x01;
            Int16[] pumpspeed = ((PumpBoard)o).GetPumpspeed();
            for (i = 0; i < 8; i++)
            {
                SerialTxBuf[2 * i + 8] = (byte)(pumpspeed[i]);
                SerialTxBuf[2 * i + 9] = (byte)(pumpspeed[i] >> 8);
            }
            for (i = 0; i < 24; i++) TxSum += SerialTxBuf[i];
            TxSum = (byte)((~TxSum) + 1);
            SerialTxBuf[3] = TxSum;
            if (((PumpBoard)o).Port == 1)
            {
                if (ComPort1.IsOpen == true) ComPort1.Write(SerialTxBuf, 0, 24);
            }
            else if (((PumpBoard)o).Port == 2)
            {
                if (ComPort2.IsOpen == true) ComPort2.Write(SerialTxBuf, 0, 24);
            }
        }

        private void AirboxSend(object o)
        {
            int i;
            byte TxSum;
            byte[] SerialTxBuf = new byte[64];

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
            if (((Airbox)o).Port == 1)
            {
                if (ComPort1.IsOpen == true) ComPort1.Write(SerialTxBuf, 0, 12);
            }
            else if (((Airbox)o).Port == 2)
            {
                if (ComPort2.IsOpen == true) ComPort2.Write(SerialTxBuf, 0, 12);
            }
        }

        private void CO2flapSend(object o)
        {
            int i;
            byte TxSum;
            byte[] SerialTxBuf = new byte[64];

            TxSum = 0x00;
            SerialTxBuf[0] = 0xAA;
            SerialTxBuf[1] = 0x55;
            SerialTxBuf[2] = 20;
            SerialTxBuf[3] = 0x00;
            SerialTxBuf[4] = (byte)((CO2flap)o).ID;
            SerialTxBuf[5] = (byte)((((CO2flap)o).ID) >> 8);
            SerialTxBuf[6] = 0x06;
            SerialTxBuf[7] = 0x03;
            byte[] CO2flapdata = ((CO2flap)o).GetCO2flapParam();
            for (i = 0; i < 12; i++)
                SerialTxBuf[i + 8] = CO2flapdata[i];
            for (i = 0; i < 20; i++) TxSum += SerialTxBuf[i];
            TxSum = (byte)((~TxSum) + 1);
            SerialTxBuf[3] = TxSum;
            if (((CO2flap)o).Port == 1)
            {
                if (ComPort1.IsOpen == true) ComPort1.Write(SerialTxBuf, 0, 20);
            }
            if (((CO2flap)o).Port == 2)
            {
                if (ComPort2.IsOpen == true) ComPort2.Write(SerialTxBuf, 0, 20);
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

        private void Com2InvokeFun()
        {
            UInt16 nodeID;
            UInt16 type;

            nodeID = (UInt16)InBuf2[0];
            nodeID += (UInt16)(((UInt16)InBuf2[1]) << 8);

            type = (UInt16)InBuf2[2];
            type += (UInt16)(((UInt16)InBuf2[3]) << 8);

            Com2ProcessData(nodeID, type);
            Display(nodeID, type);
        }

        private void DisplayBoardTConfig(UInt16 id)
        {
            string[] config;
            if (Boardconfig.Contains(id) == true)
            {
                int i = Boardconfig.IndexOf(CurTempNodID);
                if (((BoardConfig)Boardconfig[i]).type == (int)SensorType.TEMPBOARD)
                    config = ((BoardConfig)Boardconfig[i]).GetConfig();
                else
                {
                    config = new string[16];
                    for (i = 0; i < 16; i++)
                        config[i] = "NA";
                }
            }
            else
            {
                config = new string[16];
                for (int i = 0; i < 16; i++)
                    config[i] = "NA";
            }

            if (config[0] != "NA")
                labelTCH1.Text = config[0] + "(℃):";
            else
                labelTCH1.Text = "Chanel1(℃):";
            if (config[1] != "NA")
                labelTCH2.Text = config[1] + "(℃):";
            else
                labelTCH2.Text = "Chanel2(℃):";
            if (config[2] != "NA")
                labelTCH3.Text = config[2] + "(℃):";
            else
                labelTCH3.Text = "Chanel3(℃):";
            if (config[3] != "NA")
                labelTCH4.Text = config[3] + "(℃):";
            else
                labelTCH4.Text = "Chanel4(℃):";
            if (config[4] != "NA")
                labelTCH5.Text = config[4] + "(℃):";
            else
                labelTCH5.Text = "Chanel5(℃):";
            if (config[5] != "NA")
                labelTCH6.Text = config[5] + "(℃):";
            else
                labelTCH6.Text = "Chanel6(℃):";
            if (config[6] != "NA")
                labelTCH7.Text = config[6] + "(℃):";
            else
                labelTCH7.Text = "Chanel7(℃):";
            if (config[7] != "NA")
                labelTCH8.Text = config[7] + "(℃):";
            else
                labelTCH8.Text = "Chanel8(℃):";
            if (config[8] != "NA")
                labelTCH9.Text = config[8] + "(℃):";
            else
                labelTCH9.Text = "Chanel9(℃):";
            if (config[9] != "NA")
                labelTCH10.Text = config[9] + "(℃):";
            else
                labelTCH10.Text = "Chanel10(℃):";
            if (config[10] != "NA")
                labelTCH11.Text = config[10] + "(℃):";
            else
                labelTCH11.Text = "Chanel11(℃):";
            if (config[11] != "NA")
                labelTCH12.Text = config[11] + "(℃):";
            else
                labelTCH12.Text = "Chanel12(℃):";
            if (config[12] != "NA")
                labelTCH13.Text = config[12] + "(℃):";
            else
                labelTCH13.Text = "Chanel13(℃):";
            if (config[13] != "NA")
                labelTCH14.Text = config[13] + "(℃):";
            else
                labelTCH14.Text = "Chanel4(℃):";
            if (config[14] != "NA")
                labelTCH15.Text = config[14] + "(℃):";
            else
                labelTCH15.Text = "Chanel15(℃):";
            if (config[15] != "NA")
                labelTCH16.Text = config[15] + "(℃):";
            else
                labelTCH16.Text = "Chanel16(℃):";
        }

        private void DisplayBoardPConfig(UInt16 id)
        {
            string[] config;
            if (Boardconfig.Contains(id) == true)
            {
                int i = Boardconfig.IndexOf(CurPumpNodeID);
                if( ((BoardConfig)Boardconfig[i]).type == (int)SensorType.PUMPBOARD )
                    config = ((BoardConfig)Boardconfig[i]).GetConfig();
                else
                {
                    config = new string[16];
                    for (i = 0; i < 16; i++)
                        config[i] = "NA";
                }
            }
            else
            {
                config = new string[16];
                for (int i = 0; i < 16; i++)
                    config[i] = "NA";
            }

            if (config[0] != "NA")
                labelFCH1.Text = config[0] + "(mL/s):";
            else
                labelFCH1.Text = "Chanel1(mL/s):";
            if (config[1] != "NA")
                labelFCH2.Text = config[1] + "(mL/s):";
            else
                labelFCH2.Text = "Chanel2(mL/s):";
            if (config[2] != "NA")
                labelFCH3.Text = config[2] + "(mL/s):";
            else
                labelFCH3.Text = "Chanel3(mL/s):";
            if (config[3] != "NA")
                labelFCH4.Text = config[3] + "(mL/s):";
            else
                labelFCH4.Text = "Chanel4(mL/s):";
            if (config[4] != "NA")
                labelFCH5.Text = config[4] + "(mL/s):";
            else
                labelFCH5.Text = "Chanel5(mL/s):";
            if (config[5] != "NA")
                labelFCH6.Text = config[5] + "(mL/s):";
            else
                labelFCH6.Text = "Chanel6(mL/s):";
            if (config[6] != "NA")
                labelFCH7.Text = config[6] + "(mL/s):";
            else
                labelFCH7.Text = "Chanel7(mL/s):";
            if (config[7] != "NA")
                labelFCH8.Text = config[7] + "(mL/s):";
            else
                labelFCH8.Text = "Chanel8(mL/s):";

            if (config[8] != "NA")
                labelPump1.Text = config[8] + ":";
            else
                labelPump1.Text = "Pump1:";
            if (config[9] != "NA")
                labelPump2.Text = config[9] + ":";
            else
                labelPump2.Text = "Pump2:";
            if (config[10] != "NA")
                labelPump3.Text = config[10] + ":";
            else
                labelPump3.Text = "Pump3:";
            if (config[11] != "NA")
                labelPump4.Text = config[11] + ":";
            else
                labelPump4.Text = "Pump4:";
            if (config[12] != "NA")
                labelPump5.Text = config[12] + ":";
            else
                labelPump5.Text = "Pump5:";
            if (config[13] != "NA")
                labelPump6.Text = config[13] + ":";
            else
                labelPump6.Text = "Pump6:";
            if (config[14] != "NA")
                labelPump7.Text = config[14] + ":";
            else
                labelPump7.Text = "Pump7:";
            if (config[15] != "NA")
                labelPump8.Text = config[15] + ":";
            else
                labelPump8.Text = "Pump8:";
        }

        private void Com1ProcessData(UInt16 nodeID, UInt16 type)
        {
            int i = 0;

            switch (type)
            {
                case 0x0000:
                case 0x0001:
                    if (TempBoards.Contains(nodeID) == false)
                    {
                        if (Temperatureconfig.Contains(nodeID) == true)
                        {
                            int index = Temperatureconfig.IndexOf(nodeID);
                            TempBoards.Add(new TemperatueBoard(nodeID,((TemperatureConfig)Temperatureconfig[index])));
                        }
                        else
                            TempBoards.Add(new TemperatueBoard(nodeID));
                        comboBoxT.Items.Add(nodeID);

                        if (CurTempNodID == 0)
                        {
                            CurTempNodID = nodeID;
                            comboBoxT.SelectedItem = nodeID;
                            DisplayBoardTConfig(CurTempNodID);
                        }
                    }
                    i = TempBoards.IndexOf(nodeID);
                    ((TemperatueBoard)TempBoards[i]).SetTemperature(InBuf1, (type & 0x01));
                    ((TemperatueBoard)TempBoards[i]).Port = 1;
                    ((TemperatueBoard)TempBoards[i]).Online_T = true;
                    break;
                case 0x0100:
                    if (PumpBoards.Contains(nodeID) == false)
                    {
                        if (Flowrateconfig.Contains(nodeID) == true )
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
                            DisplayBoardPConfig(CurPumpNodeID);
                        }
                    }
                    i = PumpBoards.IndexOf(nodeID);
                    ((PumpBoard)PumpBoards[i]).SetFlowrate(InBuf1);
                    ((PumpBoard)PumpBoards[i]).Port = 1;
                    ((PumpBoard)PumpBoards[i]).Online_T = true;
                    break;
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
                            for (int m = 0; m < 4; m++) InBuf1[m+4] = InBuf1[m + 8];
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
                            for (int m = 0; m < 4; m++) InBuf1[m+4] = InBuf1[m + 8];
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
                            for (int m = 0; m < 4; m++) InBuf1[m+4] = InBuf1[m + 8];
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
                            for (int m = 0; m < 4; m++) InBuf1[m+4] = InBuf1[m + 8];
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf1);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 1;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            break;
                        default:
                            break;

                    }
                    break;
                default:
                    return;
            }

        }

        private void Com2ProcessData(UInt16 nodeID, UInt16 type)
        {
            int i = 0;

            switch (type)
            {
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
                            DisplayBoardTConfig(CurTempNodID);
                        }
                    }
                    i = TempBoards.IndexOf(nodeID);
                    ((TemperatueBoard)TempBoards[i]).SetTemperature(InBuf2, (type & 0x01));
                    ((TemperatueBoard)TempBoards[i]).Port = 2;
                    ((TemperatueBoard)TempBoards[i]).Online_T = true;
                    break;
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
                            DisplayBoardPConfig(CurPumpNodeID);
                        }
                    }
                    i = PumpBoards.IndexOf(nodeID);
                    ((PumpBoard)PumpBoards[i]).SetFlowrate(InBuf2);
                    ((PumpBoard)PumpBoards[i]).Port = 2;                    
                    ((PumpBoard)PumpBoards[i]).Online_T = true;
                    break;
                case 0x0200:
                    if (Airboxes.Contains(nodeID) == false)
                    {
                        Airboxes.Add(new Airbox(nodeID));
                    }
                    i = Airboxes.IndexOf(nodeID);
                    ((Airbox)Airboxes[i]).SetAirboxdata(InBuf2);
                    ((Airbox)Airboxes[i]).Port = 2;
                    ((Airbox)Airboxes[i]).Online_T = true;
                    break;
                case 0x0300:
                    if (CO2flaps.Contains(nodeID) == false)
                    {
                        CO2flaps.Add(new CO2flap(nodeID));
                    }
                    i = CO2flaps.IndexOf(nodeID);
                    ((CO2flap)CO2flaps[i]).SetCO2flapdata(InBuf2);
                    ((CO2flap)CO2flaps[i]).Port = 2;
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
                    ((TelosbSensor)TelosbSensors[i]).SetTelosbdata(InBuf2);
                    ((TelosbSensor)TelosbSensors[i]).Port = 2;
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
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf2);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 2;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            i = AirboxSht75s.IndexOf((UInt16)2);
                            for (int m = 0; m < 4; m++) InBuf2[m + 4] = InBuf2[m + 8];
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf2);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 2;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            break;
                        case 26:
                            if (AirboxSht75s.Contains((UInt16)3) == false)
                            {
                                AirboxSht75s.Add(new TelosbSensor((UInt16)3));
                                AirboxSht75s.Add(new TelosbSensor((UInt16)4));
                            }
                            i = AirboxSht75s.IndexOf((UInt16)3);
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf2);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 2;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            i = AirboxSht75s.IndexOf((UInt16)4);
                            for (int m = 0; m < 4; m++) InBuf2[m + 4] = InBuf2[m + 8];
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf2);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 2;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            break;
                        case 27:
                            if (AirboxSht75s.Contains((UInt16)5) == false)
                            {
                                AirboxSht75s.Add(new TelosbSensor((UInt16)5));
                                AirboxSht75s.Add(new TelosbSensor((UInt16)6));
                            }
                            i = AirboxSht75s.IndexOf((UInt16)5);
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf2);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 2;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            i = AirboxSht75s.IndexOf((UInt16)6);
                            for (int m = 0; m < 4; m++) InBuf2[m + 4] = InBuf2[m + 8];
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf2);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 2;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            break;
                        case 28:
                            if (AirboxSht75s.Contains((UInt16)7) == false)
                            {
                                AirboxSht75s.Add(new TelosbSensor((UInt16)7));
                                AirboxSht75s.Add(new TelosbSensor((UInt16)8));
                            }
                            i = AirboxSht75s.IndexOf((UInt16)7);
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf2);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 2;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            i = AirboxSht75s.IndexOf((UInt16)8);
                            for (int m = 0; m < 4; m++) InBuf2[m + 4] = InBuf2[m + 8];
                            ((TelosbSensor)AirboxSht75s[i]).SetTelosbdata(InBuf2);
                            ((TelosbSensor)AirboxSht75s[i]).Port = 2;
                            ((TelosbSensor)AirboxSht75s[i]).Online_T = true;
                            break;
                        default:
                            break;

                    }
                    break;
                default:
                    return;
            }

        }

        private void Display(UInt16 nodeID, UInt16 type)
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
                                textBoxTCH1.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[1] == 0x4000)
                            {
                                textBoxTCH2.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[1] / 16.0;
                                textBoxTCH2.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[2] == 0x4000)
                            {
                                textBoxTCH3.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[2] / 16.0;
                                textBoxTCH3.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[3] == 0x4000)
                            {
                                textBoxTCH4.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[3] / 16.0;
                                textBoxTCH4.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[4] == 0x4000)
                            {
                                textBoxTCH5.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[4] / 16.0;
                                textBoxTCH5.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[5] == 0x4000)
                            {
                                textBoxTCH6.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[5] / 16.0;
                                textBoxTCH6.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[6] == 0x4000)
                            {
                                textBoxTCH7.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[6] / 16.0;
                                textBoxTCH7.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[7] == 0x4000)
                            {
                                textBoxTCH8.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[7] / 16.0;
                                textBoxTCH8.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[8] == 0x4000)
                            {
                                textBoxTCH9.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[8] / 16.0;
                                textBoxTCH9.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[9] == 0x4000)
                            {
                                textBoxTCH10.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[9] / 16.0;
                                textBoxTCH10.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[10] == 0x4000)
                            {
                                textBoxTCH11.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[10] / 16.0;
                                textBoxTCH11.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[11] == 0x4000)
                            {
                                textBoxTCH12.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[11] / 16.0;
                                textBoxTCH12.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[12] == 0x4000)
                            {
                                textBoxTCH13.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[12] / 16.0;
                                textBoxTCH13.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[13] == 0x4000)
                            {
                                textBoxTCH14.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[13] / 16.0;
                                textBoxTCH14.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[14] == 0x4000)
                            {
                                textBoxTCH15.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[14] / 16.0;
                                textBoxTCH15.Text = string.Format("{0:0.000}", tmp);
                            }

                            if (temperature[15] == 0x4000)
                            {
                                textBoxTCH16.Text = "NC";
                            }
                            else
                            {
                                tmp = temperature[15] / 16.0;
                                textBoxTCH16.Text = string.Format("{0:0.000}", tmp);
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
                            double[] flowrate = ((PumpBoard)PumpBoards[i]).GetFlowrate();
                            textBoxFCH1.Text = string.Format("{0:0.000}", flowrate[0]);
                            textBoxFCH2.Text = string.Format("{0:0.000}", flowrate[1]);
                            textBoxFCH3.Text = string.Format("{0:0.000}", flowrate[2]);
                            textBoxFCH4.Text = string.Format("{0:0.000}", flowrate[3]);
                            textBoxFCH5.Text = string.Format("{0:0.000}", flowrate[4]);
                            textBoxFCH6.Text = string.Format("{0:0.000}", flowrate[5] );
                            textBoxFCH7.Text = string.Format("{0:0.000}", flowrate[6]);
                            textBoxFCH8.Text = string.Format("{0:0.000}", flowrate[7]);
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
            if ((status & 0x04) == 0x04)
                ret += "PPM exceed limit value ";
            else
                ret += "PPM below limit value ";
            if ((status & 0x20) == 0x20)
                ret += "flap open ";
            else
                ret += "flap closed ";
            return ret;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (enSync == true && SyncFolder != "")
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
                    foreach (object o in AirboxSht75s)
                    {
                        origin = Directory.GetCurrentDirectory() + "\\Data\\Sht75" + ((TelosbSensor)o).ID.ToString() + ".txt";
                        dest = SyncFolder + "\\Sht75" + ((TelosbSensor)o).ID.ToString() + ".txt";
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

                    if (TempBoards.Contains((UInt16)2) == true)
                    {
                        origin = Directory.GetCurrentDirectory() + "\\Data\\Power.txt";
                        dest = SyncFolder + "\\Power.txt";
                        if (File.Exists(origin) == true)
                            File.Copy(origin, dest, true);
                    }
                }
                catch
                {
                }
            }

            
        }

        private void buttonPort1_Click(object sender, EventArgs e)
        {
            threadCom1RecvEn = false;
            threadComSendEn = false;
            if (threadCom1Recv.IsAlive) 
                threadCom1Recv.Join();
            if (threadComSend.IsAlive)
                threadComSend.Join();
            ComSet comset = new ComSet(ref this.ComPort1);
            comset.ShowDialog();
            if (ComPort1.IsOpen == true)
                ComPort1.DiscardInBuffer();
            ComPort1.ReadTimeout = 50;
            threadCom1RecvEn = true;
            threadCom1Recv = new Thread(ThreadCom1Recv);
            threadCom1Recv.Start();
            threadComSendEn = true;
            threadComSend = new Thread(ThreadComSend);
            threadComSend.Start();
        }

        private void buttonPort2_Click(object sender, EventArgs e)
        {
            threadCom2RecvEn = false;
            threadComSendEn = false;
            if(threadCom2Recv.IsAlive)
                threadCom2Recv.Join();
            if (threadComSend.IsAlive)
                threadComSend.Join();
            ComSet comset = new ComSet(ref this.ComPort2);
            comset.ShowDialog();
            if (ComPort2.IsOpen == true)
                ComPort2.DiscardInBuffer();
            ComPort2.ReadTimeout = 50;
            threadCom2RecvEn = true;
            threadCom2Recv = new Thread(ThreadCom2Recv);
            threadCom2Recv.Start();
            threadComSendEn = true;
            threadComSend = new Thread(ThreadComSend);
            threadComSend.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            threadCom1RecvEn = false;
            threadCom2RecvEn = false;
            threadComSendEn = false;

            if (threadCom1Recv.IsAlive) 
                threadCom1Recv.Join();
            if (threadCom2Recv.IsAlive)
                threadCom2Recv.Join();
            if (threadComSend.IsAlive)
                threadComSend.Join();
            if (threadUpload != null && threadUpload.IsAlive)
                threadUpload.Abort();            
        }

        private void comboBoxT_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurTempNodID = UInt16.Parse(comboBoxT.SelectedItem.ToString());
            DisplayBoardTConfig(CurTempNodID);
        }

        private void comboBoxP_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurPumpNodeID = UInt16.Parse(comboBoxP.SelectedItem.ToString());
            DisplayBoardPConfig(CurPumpNodeID);
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                double[] pumpvalue = ((PumpBoard)PumpBoards[i]).GetPumpvalue();
                trackBarCH1.Value = (int)pumpvalue[0];
                trackBarCH2.Value = (int)pumpvalue[1];
                trackBarCH3.Value = (int)pumpvalue[2];
                trackBarCH4.Value = (int)pumpvalue[3];
                trackBarCH5.Value = (int)pumpvalue[4];
                trackBarCH6.Value = (int)pumpvalue[5];
                trackBarCH7.Value = (int)pumpvalue[6];
                trackBarCH8.Value = (int)pumpvalue[7];  
                textBoxPCH1.Text = string.Format("{0}", pumpvalue[0] * 10);
                textBoxPCH2.Text = string.Format("{0}", pumpvalue[1] * 10);
                textBoxPCH3.Text = string.Format("{0}", pumpvalue[2] * 10);
                textBoxPCH4.Text = string.Format("{0}", pumpvalue[3] * 10);
                textBoxPCH5.Text = string.Format("{0}", pumpvalue[4] * 10);
                textBoxPCH6.Text = string.Format("{0}", pumpvalue[5] * 10);
                textBoxPCH7.Text = string.Format("{0}", pumpvalue[6] * 10);
                textBoxPCH8.Text = string.Format("{0}", pumpvalue[7] * 10);
            }
        }

       
        private void trackBarCH1_Scroll(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH1.Value,1);                
            }
            textBoxPCH1.Text = (trackBarCH1.Value * 10).ToString();
        }


        private void trackBarCH2_Scroll(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH2.Value, 2);
            }
            textBoxPCH2.Text = (trackBarCH2.Value * 10).ToString();
        }

        private void trackBarCH3_Scroll(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH3.Value, 3);
            }            
            textBoxPCH3.Text = (trackBarCH3.Value * 10).ToString();
        }


        private void trackBarCH4_Scroll(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH4.Value, 4);
            }  
            textBoxPCH4.Text = (trackBarCH4.Value * 10).ToString();
        }


        private void trackBarCH5_Scroll(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH5.Value, 5);
            }  
            textBoxPCH5.Text = (trackBarCH5.Value * 10).ToString();
        }

        private void trackBarCH6_Scroll(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH6.Value, 6);
            }  
            textBoxPCH6.Text = (trackBarCH6.Value * 10).ToString();
        }

        private void trackBarCH7_Scroll(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH7.Value, 7);
            }  
            textBoxPCH7.Text = (trackBarCH7.Value * 10).ToString();
        }


        private void trackBarCH8_Scroll(object sender, EventArgs e)
        {
            int i = PumpBoards.IndexOf(CurPumpNodeID);
            if (i >= 0)
            {
                ((PumpBoard)PumpBoards[i]).SetPumpvalue(trackBarCH8.Value, 8);
            }  
            textBoxPCH8.Text = (trackBarCH8.Value * 10).ToString();
        }

        private void textboxPCH1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int i = PumpBoards.IndexOf(CurPumpNodeID);
                if (i >= 0)
                {
                    double j = 0.0;
                    try
                    {
                        j = double.Parse(textBoxPCH1.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                    if (j >= 0.0 && j <= 100.0)
                    {
                        j = j / 10;
                        ((PumpBoard)PumpBoards[i]).SetPumpvalue(j, 1);
                        trackBarCH1.Value = (int)j;
                    }
                    else
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                }
            }
        }

        private void textboxPCH2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int i = PumpBoards.IndexOf(CurPumpNodeID);
                if (i >= 0)
                {
                    double j = 0.0;
                    try
                    {
                        j = double.Parse(textBoxPCH2.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                    if (j >= 0.0 && j <= 100.0)
                    {
                        j = j / 10;
                        ((PumpBoard)PumpBoards[i]).SetPumpvalue(j, 2);
                        trackBarCH2.Value = (int)j;
                    }
                    else
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                }
            }
        }
        private void textboxPCH3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int i = PumpBoards.IndexOf(CurPumpNodeID);
                if (i >= 0)
                {
                    double j = 0.0;
                    try
                    {
                        j = double.Parse(textBoxPCH3.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                    if (j >= 0.0 && j <= 100.0)
                    {
                        j = j / 10;
                        ((PumpBoard)PumpBoards[i]).SetPumpvalue(j, 3);
                        trackBarCH3.Value = (int)j;
                    }
                    else
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                }
            }
        }
        private void textboxPCH4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int i = PumpBoards.IndexOf(CurPumpNodeID);
                if (i >= 0)
                {
                    double j = 0.0;
                    try
                    {
                        j = double.Parse(textBoxPCH4.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                    if (j >= 0.0 && j <= 100.0)
                    {
                        j = j / 10;
                        ((PumpBoard)PumpBoards[i]).SetPumpvalue(j, 4);
                        trackBarCH4.Value = (int)j;
                    }
                    else
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                }
            }
        }
        private void textboxPCH5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int i = PumpBoards.IndexOf(CurPumpNodeID);
                if (i >= 0)
                {
                    double j = 0.0;
                    try
                    {
                        j = double.Parse(textBoxPCH5.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                    if (j >= 0.0 && j <= 100.0)
                    {
                        j = j / 10;
                        ((PumpBoard)PumpBoards[i]).SetPumpvalue(j, 5);
                        trackBarCH5.Value = (int)j;
                    }
                    else
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                }
            }
        }
        private void textboxPCH6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int i = PumpBoards.IndexOf(CurPumpNodeID);
                if (i >= 0)
                {
                    double j = 0.0;
                    try
                    {
                        j = double.Parse(textBoxPCH6.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                    if (j >= 0.0 && j <= 100.0)
                    {
                        j = j / 10;
                        ((PumpBoard)PumpBoards[i]).SetPumpvalue(j, 6);
                        trackBarCH6.Value = (int)j;
                    }
                    else
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                }
            }
        }
        private void textboxPCH7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int i = PumpBoards.IndexOf(CurPumpNodeID);
                if (i >= 0)
                {
                    double j = 0.0;
                    try
                    {
                        j = double.Parse(textBoxPCH7.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                    if (j >= 0.0 && j <= 100.0)
                    {
                        j = j / 10;
                        ((PumpBoard)PumpBoards[i]).SetPumpvalue(j, 7);
                        trackBarCH7.Value = (int)j;
                    }
                    else
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                }
            }
        }
        private void textboxPCH8_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int i = PumpBoards.IndexOf(CurPumpNodeID);
                if (i >= 0)
                {
                    double j = 0.0;
                    try
                    {
                        j = double.Parse(textBoxPCH8.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                    if (j >= 0.0 && j <= 100.0)
                    {
                        j = j / 10;
                        ((PumpBoard)PumpBoards[i]).SetPumpvalue(j, 8);
                        trackBarCH8.Value = (int)j;
                    }
                    else
                    {
                        MessageBox.Show("Wrong input: Please input a positive float number between 0-100");
                    }
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (enStore == true)
            {
                if (overwrite == true)
                {
                    foreach (object o in TempBoards)
                    {
                        ((TemperatueBoard)o).UploadCreateNewFile = true;
                    }
                    foreach (object o in PumpBoards)
                    {
                        ((PumpBoard)o).UploadCreateNewFile = true;
                    }
                    foreach (object o in Airboxes)
                    {
                        ((Airbox)o).UploadCreateNewFile = true;
                    }
                    foreach (object o in CO2flaps)
                    {
                        ((CO2flap)o).UploadCreateNewFile = true;

                    }
                    foreach (object o in TelosbSensors)
                    {
                        ((TelosbSensor)o).UploadCreateNewFile = true;
                    }
                    overwrite = false;
                }

                CaculateSave();
                FileSave();
            }
            
            if (shouldUpload == true)
            {                
                threadUpload = new Thread(ThreadUpload);
                threadUpload.Start();
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (enUpload == true)
            {
                shouldUpload = true;
                if (threadUpload != null && threadUpload.IsAlive == true)
                {
                    threadUpload.Abort();
                }
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
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


        private void FileSave()
        {
            string filename;
            FileStream fs;
            StreamWriter sw;
            var fileMode = FileMode.Append;
            string content;
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;         
            
            foreach (object o in TempBoards)
            {
                if (((TemperatueBoard)o).Online == true && ((TemperatueBoard)o).NewInData == true)
                {
                    content = string.Format("{0:yyyy-MM-dd HH:mm:ss}", currentTime);
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

            foreach (object o in PumpBoards)
            {         
                if (((PumpBoard)o).Online == true && ((PumpBoard)o).NewInData == true)
                {
                    content = string.Format("{0:yyyy-MM-dd HH:mm:ss}", currentTime);
                    double[] flowrate = ((PumpBoard)o).GetFlowrate();
                    for (int i = 0; i < 8; i++)
                    {
                        content += ";";
                        content += string.Format("{0:0.00}", flowrate[i]);
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

            foreach (object o in Airboxes)
            {
                if (((Airbox)o).Online == true && ((Airbox)o).NewInData == true)
                {               
                    byte[] airboxdata = ((Airbox)o).GetAirboxdata();
                    content = string.Format("{0:yyyy-MM-dd HH:mm:ss}", currentTime);
                    content += string.Format(";{0:0.0}", (airboxdata[3] * 0.5));
                    content += string.Format(";{0}", (airboxdata[4] * 50) );
                    content += string.Format(";{0:0.0}", (airboxdata[5] * 0.2).ToString() );
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

            foreach (object o in AirboxSht75s)
            {
                if (((TelosbSensor)o).Online == true && ((TelosbSensor)o).NewInData == true)
                {
                    content = string.Format("{0:yyyy-MM-dd HH:mm:ss}", currentTime);
                    content += string.Format(";{0:0.00}", ((TelosbSensor)o).Temperature);
                    content += string.Format(";{0:0.00}", ((TelosbSensor)o).Humidity);
                    content += string.Format(";{0:0.00}", ((TelosbSensor)o).DewPoint);
                    content += "\n";

                    filename = datastoredirectory + "\\Sht75" + ((TelosbSensor)o).ID.ToString() + ".txt";
                    fileMode = File.Exists(filename) ? FileMode.Append : FileMode.Create;
                    fs = new FileStream(filename, fileMode, FileAccess.Write);
                    sw = new StreamWriter(fs, Encoding.Default);
                    sw.Write(content);
                    sw.Close();
                    fs.Close();

                    filename = uploadstoredirectory + "\\Sht75" + ((TelosbSensor)o).ID.ToString() + ".txt";
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
            

            foreach (object o in CO2flaps)
            {
                if (((CO2flap)o).Online == true && ((CO2flap)o).NewInData == true)
                {                                     
                    byte[] CO2flapdata = ((CO2flap)o).GetCO2flapdata();
                    content = string.Format("{0:yyyy-MM-dd HH:mm:ss}", currentTime);
                    content += string.Format(";{0:0.0}", (CO2flapdata[8] * 0.2));
                    content += string.Format(";{0}", (CO2flapdata[7] * 10));
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

            foreach (object o in TelosbSensors)
            {
                if (((TelosbSensor)o).Online == true && ((TelosbSensor)o).NewInData == true)
                {
                    content = string.Format("{0:yyyy-MM-dd HH:mm:ss}", currentTime);
                    content += string.Format(";{0:0.00}", ((TelosbSensor)o).Temperature);
                    content += string.Format(";{0:0.00}", ((TelosbSensor)o).Humidity);
                    content += string.Format(";{0:0.00}", ((TelosbSensor)o).DewPoint);
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

        private void CaculateSave()
        {
            string filename;
            FileStream fs;
            StreamWriter sw;
            var fileMode = FileMode.Append;
            double power;
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            string content = string.Format("{0:yyyy-MM-dd HH:mm:ss;}", currentTime);

            if(TempBoards.Contains( (UInt16)2 ) && PumpBoards.Contains( (UInt16)11 ) )
            {
                int i = TempBoards.IndexOf( (UInt16)2 );
                int j = PumpBoards.IndexOf( (UInt16)11 );
                if( ((TemperatueBoard)TempBoards[i]).NewInData && ((PumpBoard)PumpBoards[j]).NewInData )
                {
                    UInt16[] temperature = ((TemperatueBoard)TempBoards[i]).GetTemperature();
                    double[] flowrate = ((PumpBoard)PumpBoards[j]).GetFlowrate();
                    if(temperature[0] != 0x4000 && temperature[1] != 0x4000)
                    {
                        power = (temperature[1]-temperature[0])*(flowrate[0]+flowrate[1])*4.2/16.0;
                        content += string.Format("{0:0.00}",power)+";";
                    }
                    else
                        content += "NA;";
                    if(temperature[2] != 0x4000 && temperature[3] != 0x4000)
                    {
                        power = (temperature[3]-temperature[2])*(flowrate[2]+flowrate[3])*4.2/16.0/9.4;
                        content += string.Format("{0:0.00}", power) + ";";
                    }
                    else
                        content += "NA;";
                }
            }           

            if(TempBoards.Contains( (UInt16)2 ) && PumpBoards.Contains( (UInt16)12 ) )
            {
                int i = TempBoards.IndexOf( (UInt16)2 );
                int j = PumpBoards.IndexOf( (UInt16)12 );
                if( ((TemperatueBoard)TempBoards[i]).NewInData && ((PumpBoard)PumpBoards[j]).NewInData )
                {
                    UInt16[] temperature = ((TemperatueBoard)TempBoards[i]).GetTemperature();
                    double[] flowrate = ((PumpBoard)PumpBoards[j]).GetFlowrate();
                    if(temperature[4] != 0x4000 && temperature[8] != 0x4000)
                    {
                        power = (temperature[4] - temperature[8]) * flowrate[0] * 4.2 / 16.0;
                        content += string.Format("{0:0.000}",power)+";";
                    }
                    else
                        content += "NA;";
                    if (temperature[5] != 0x4000 && temperature[9] != 0x4000)
                    {
                        power = (temperature[5] - temperature[9]) * flowrate[1] * 4.2 / 16.0 / 9.4;
                        content += string.Format("{0:0.000}", power) + ";";
                    }
                    else
                        content += "NA;";
                    if (temperature[6] != 0x4000 && temperature[10] != 0x4000)
                    {
                        power = (temperature[6] - temperature[10]) * flowrate[2] * 4.2 / 16.0 / 9.4;
                        content += string.Format("{0:0.000}", power) + ";";
                    }
                    else
                        content += "NA;";
                    if (temperature[7] != 0x4000 && temperature[11] != 0x4000)
                    {
                        power = (temperature[7] - temperature[11]) * flowrate[3] * 4.2 / 16.0 / 9.4;
                        content += string.Format("{0:0.000}", power) + ";";
                    }
                    else
                        content += "NA;";
                    if (temperature[14] != 0x4000 && temperature[15] != 0x4000)
                    {
                        power = (temperature[15] - temperature[14]) * flowrate[4] * 4.2 / 16.0 / 9.4;
                        content += string.Format("{0:0.000}", power) + ";";
                    }
                    else
                        content += "NA;";
                }

                content += "\r\n";
                filename = Directory.GetCurrentDirectory() + "\\Data\\Power.txt";
                fileMode = File.Exists(filename) ? FileMode.Append : FileMode.Create;
                fs = new FileStream(filename, fileMode, FileAccess.Write);
                sw = new StreamWriter(fs, Encoding.Default);
                sw.Write(content);
                sw.Close();
                fs.Close();                
            }
        }

        private void textBoxSync_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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

                if (i <= 0)
                {
                    MessageBox.Show("Wrong input");
                    textBoxSync.Text = SyncInterval.ToString();
                    return;
                }

                SyncInterval = i;
                timerSync.Stop();
                timerSync.Interval = i * 60 * 1000;
                timerSync.Start();
            }
        }

        private void textBoxStrInterval_KeyDown(object sender, KeyEventArgs e)
        {            
            if (e.KeyCode == Keys.Enter)
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
                timerStore.Stop();
                timerStore.Interval = i * 1000;
                timerStore.Start();
            }
        }

        private void textBoxUpload_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int i = 0;
                try
                {
                    i = int.Parse(textBoxUpload.Text);
                }
                catch
                {
                    if (textBoxSync.Text != "")
                    {
                        MessageBox.Show("Wrong input");
                        textBoxUpload.Text = UploadInterval.ToString();
                    }
                    return;
                }

                if (i <= 0)
                {
                    MessageBox.Show("Wrong input");
                    textBoxUpload.Text = UploadInterval.ToString();
                    return;
                }

                UploadInterval = i;
                timerUpload.Stop();
                timerUpload.Interval = i * 60 * 1000;
                timerUpload.Start();
            }
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

        private void textBoxFP1PPM_THRES_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP1PPM_HYST_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP1TEMP_THRES_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP1TEMP_HYST_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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

        private void textBoxFP2PPM_THRES_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP2PPM_HYST_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP2TEMP_THRES_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP2TEMP_HYST_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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

        private void textBoxFP3PPM_THRES_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP3PPM_HYST_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP3TEMP_THRES_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP3TEMP_HYST_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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

        private void textBoxFP4PPM_THRES_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP4PPM_HYST_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP4TEMP_THRES_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void textBoxFP4TEMP_HYST_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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

        private void buttonFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            SyncFolder = folderBrowserDialog1.SelectedPath;
        }

        private void checkBoxSync_CheckedChanged(object sender, EventArgs e)
        {
            enSync = checkBoxSync.Checked;
        }

        private void checkBoxUpload_CheckedChanged(object sender, EventArgs e)
        {
            enUpload = checkBoxUpload.Checked;
        }

        private void checkBoxStore_CheckedChanged(object sender, EventArgs e)
        {
            enStore = checkBoxStore.Checked;
        }

        private void checkBoxFlap1_CheckedChanged(object sender, EventArgs e)
        {
            if(CO2flaps.Contains((UInt16)31))
            {
                int i = CO2flaps.IndexOf((UInt16)31);
                ((CO2flap)CO2flaps[i]).FlapAuto = checkBoxFlap1.Checked;
                if (checkBoxFlap1.Checked == true)
                {
                    buttonFP1.Enabled = false;
                }
                else
                {
                    buttonFP1.Enabled = true;
                }
            }
        }

        private void checkBoxFlap2_CheckedChanged(object sender, EventArgs e)
        {
            if (CO2flaps.Contains((UInt16)32))
            {
                int i = CO2flaps.IndexOf((UInt16)32);
                ((CO2flap)CO2flaps[i]).FlapAuto = checkBoxFlap2.Checked;
                if (checkBoxFlap2.Checked == true)
                {
                    buttonFP2.Enabled = false;
                }
                else
                {
                    buttonFP2.Enabled = true;
                }
            }
        }

        private void checkBoxFlap3_CheckedChanged(object sender, EventArgs e)
        {
            if (CO2flaps.Contains((UInt16)33))
            {
                int i = CO2flaps.IndexOf((UInt16)33);
                ((CO2flap)CO2flaps[i]).FlapAuto = checkBoxFlap3.Checked;
                if (checkBoxFlap3.Checked == true)
                {
                    buttonFP3.Enabled = false;
                }
                else
                {
                    buttonFP3.Enabled = true;
                }
            }
        }

        private void checkBoxFlap4_CheckedChanged(object sender, EventArgs e)
        {
            if (CO2flaps.Contains((UInt16)34))
            {
                int i = CO2flaps.IndexOf((UInt16)34);
                ((CO2flap)CO2flaps[i]).FlapAuto = checkBoxFlap4.Checked;
                if (checkBoxFlap4.Checked == true)
                {
                    buttonFP4.Enabled = false;
                }
                else
                {
                    buttonFP4.Enabled = true;
                }
            }
        }
    }
}

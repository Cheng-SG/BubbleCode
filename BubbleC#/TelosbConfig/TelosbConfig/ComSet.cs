using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace TelosbConfig
{
    public partial class ComSet : Form
    {
        private SerialPort CommunicationPort;
        public ComSet(ref SerialPort m)
        {
            InitializeComponent();
            CommunicationPort = m;
        }

        private void ComSet_Load(object sender, EventArgs e)
        {
            string[] ExistPort = SerialPort.GetPortNames();
            if (ExistPort.Length < 1)
            {
                MessageBox.Show("您的电脑没有串口！");
                this.Close();
            }
            else
            {
                IEnumerable<string> query = from word in ExistPort
                                            orderby word.Substring(0, 4)
                                            select word;
                foreach (string str in query)
                {
                    this.cbPort.Items.Add(str);
                }
                this.cbBaudrate.SelectedIndex = 12;
                this.cbDataBits.SelectedIndex = 3;
                this.cbStopbits.SelectedIndex = 0;
                this.cbCheckbit.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.cbPort.SelectedIndex >= 0)
            {
                CommunicationPort.Close();
                this.CommunicationPort.PortName = this.cbPort.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("请选择端口！");
                return;
            }
            this.CommunicationPort.BaudRate = Convert.ToInt32(this.cbBaudrate.SelectedItem.ToString());
            this.CommunicationPort.DataBits = Convert.ToInt32(this.cbDataBits.SelectedItem.ToString());
            switch (this.cbStopbits.SelectedItem.ToString())
            {
                case "1": this.CommunicationPort.StopBits = System.IO.Ports.StopBits.One; break;
                case "1.5": this.CommunicationPort.StopBits = System.IO.Ports.StopBits.OnePointFive; break;
                case "2": this.CommunicationPort.StopBits = System.IO.Ports.StopBits.Two; break;
            }
            switch (this.cbCheckbit.SelectedItem.ToString())
            {
                case "None": this.CommunicationPort.Parity = System.IO.Ports.Parity.None; break;
                case "Odd": this.CommunicationPort.Parity = System.IO.Ports.Parity.Odd; break;
                case "Even": this.CommunicationPort.Parity = System.IO.Ports.Parity.Even; break;
                case "Mark": this.CommunicationPort.Parity = System.IO.Ports.Parity.Mark; break;
                case "Space": this.CommunicationPort.Parity = System.IO.Ports.Parity.Space; break;
            }
            this.CommunicationPort.Open();
            this.Close();
        }
    }
}

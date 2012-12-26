namespace TelosbConfig
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelNodeT = new System.Windows.Forms.Label();
            this.comboBoxT = new System.Windows.Forms.ComboBox();
            this.textBoxDew = new System.Windows.Forms.TextBox();
            this.labelCH5 = new System.Windows.Forms.Label();
            this.textBoxHumi = new System.Windows.Forms.TextBox();
            this.labelCH3 = new System.Windows.Forms.Label();
            this.textBoxTemp = new System.Windows.Forms.TextBox();
            this.labelCH1 = new System.Windows.Forms.Label();
            this.ComPort1 = new System.IO.Ports.SerialPort(this.components);
            this.buttonPort1 = new System.Windows.Forms.Button();
            this.textBoxCFG1 = new System.Windows.Forms.TextBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelNodeT
            // 
            this.labelNodeT.AutoSize = true;
            this.labelNodeT.Location = new System.Drawing.Point(34, 59);
            this.labelNodeT.Name = "labelNodeT";
            this.labelNodeT.Size = new System.Drawing.Size(52, 13);
            this.labelNodeT.TabIndex = 101;
            this.labelNodeT.Text = "NODE ID";
            // 
            // comboBoxT
            // 
            this.comboBoxT.FormattingEnabled = true;
            this.comboBoxT.Location = new System.Drawing.Point(124, 56);
            this.comboBoxT.Name = "comboBoxT";
            this.comboBoxT.Size = new System.Drawing.Size(123, 21);
            this.comboBoxT.TabIndex = 100;
            this.comboBoxT.SelectedIndexChanged += new System.EventHandler(this.comboBoxT_SelectedIndexChanged);
            // 
            // textBoxDew
            // 
            this.textBoxDew.Location = new System.Drawing.Point(128, 202);
            this.textBoxDew.Name = "textBoxDew";
            this.textBoxDew.Size = new System.Drawing.Size(123, 20);
            this.textBoxDew.TabIndex = 77;
            // 
            // labelCH5
            // 
            this.labelCH5.AutoSize = true;
            this.labelCH5.Location = new System.Drawing.Point(34, 205);
            this.labelCH5.Name = "labelCH5";
            this.labelCH5.Size = new System.Drawing.Size(80, 13);
            this.labelCH5.TabIndex = 76;
            this.labelCH5.Text = "Dew Point (℃):";
            // 
            // textBoxHumi
            // 
            this.textBoxHumi.Location = new System.Drawing.Point(128, 151);
            this.textBoxHumi.Name = "textBoxHumi";
            this.textBoxHumi.Size = new System.Drawing.Size(123, 20);
            this.textBoxHumi.TabIndex = 73;
            // 
            // labelCH3
            // 
            this.labelCH3.AutoSize = true;
            this.labelCH3.Location = new System.Drawing.Point(34, 154);
            this.labelCH3.Name = "labelCH3";
            this.labelCH3.Size = new System.Drawing.Size(67, 13);
            this.labelCH3.TabIndex = 72;
            this.labelCH3.Text = "Humidity (%):";
            // 
            // textBoxTemp
            // 
            this.textBoxTemp.Location = new System.Drawing.Point(128, 100);
            this.textBoxTemp.Name = "textBoxTemp";
            this.textBoxTemp.Size = new System.Drawing.Size(81, 20);
            this.textBoxTemp.TabIndex = 69;
            // 
            // labelCH1
            // 
            this.labelCH1.AutoSize = true;
            this.labelCH1.Location = new System.Drawing.Point(34, 103);
            this.labelCH1.Name = "labelCH1";
            this.labelCH1.Size = new System.Drawing.Size(91, 13);
            this.labelCH1.TabIndex = 68;
            this.labelCH1.Text = "Temperature (℃):";
            // 
            // buttonPort1
            // 
            this.buttonPort1.Location = new System.Drawing.Point(37, 12);
            this.buttonPort1.Name = "buttonPort1";
            this.buttonPort1.Size = new System.Drawing.Size(83, 23);
            this.buttonPort1.TabIndex = 102;
            this.buttonPort1.Text = "Port Setting";
            this.buttonPort1.UseVisualStyleBackColor = true;
            this.buttonPort1.Click += new System.EventHandler(this.buttonPort1_Click);
            // 
            // textBoxCFG1
            // 
            this.textBoxCFG1.Location = new System.Drawing.Point(215, 100);
            this.textBoxCFG1.Name = "textBoxCFG1";
            this.textBoxCFG1.Size = new System.Drawing.Size(36, 20);
            this.textBoxCFG1.TabIndex = 103;
            this.textBoxCFG1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxCFG1_KeyDown);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(168, 12);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(83, 23);
            this.buttonSave.TabIndex = 102;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(296, 241);
            this.Controls.Add(this.textBoxCFG1);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonPort1);
            this.Controls.Add(this.labelNodeT);
            this.Controls.Add(this.comboBoxT);
            this.Controls.Add(this.textBoxDew);
            this.Controls.Add(this.labelCH5);
            this.Controls.Add(this.textBoxHumi);
            this.Controls.Add(this.labelCH3);
            this.Controls.Add(this.textBoxTemp);
            this.Controls.Add(this.labelCH1);
            this.Name = "Form1";
            this.Text = "TelosbConfig";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelNodeT;
        private System.Windows.Forms.ComboBox comboBoxT;
        private System.Windows.Forms.TextBox textBoxDew;
        private System.Windows.Forms.Label labelCH5;
        private System.Windows.Forms.TextBox textBoxHumi;
        private System.Windows.Forms.Label labelCH3;
        private System.Windows.Forms.TextBox textBoxTemp;
        private System.Windows.Forms.Label labelCH1;
        private System.IO.Ports.SerialPort ComPort1;
        private System.Windows.Forms.Button buttonPort1;
        private System.Windows.Forms.TextBox textBoxCFG1;
        private System.Windows.Forms.Button buttonSave;
    }
}


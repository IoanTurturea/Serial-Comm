using System;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;


namespace SerialCommunication_C_Sharf_
{
    public partial class Form1 : Form
    {
        public static string SetValueForTextOut = ""; 
        public static string SetValueForTextIn = "";

        string DataIn = "";
        string DataOut = "";

        Form2 f2;
        Form3 f3;
        Form4 f4;

        public Form1()
        {
            InitializeComponent();
        }

        public void Form1_Load_1(object sender, EventArgs e)
        {
            Global.OpenPort(serialPort1);
            if (serialPort1.IsOpen)
            {
                Global.AppendToFile(Global.GlobalVar.PortName + " was opened.");
                serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                btnClosePort.Enabled = true;
                btnOpenPort.Enabled = false;
            }
            else
            {
                btnClosePort.Enabled = false;
                btnOpenPort.Enabled = true;
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                DataIn = serialPort1.ReadExisting();
                Global.AppendToFile(String.Format("RECEIVED: {0}", string.Join(" ", DataIn)));

                Invoke(new EventHandler(ShowData));
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message, "Error in showing data on screen.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowData(object sender, EventArgs e)
        {
            tBoxDataIN.Text += DataIn;
            tBoxDataIN.AppendText(Environment.NewLine);

            Global.Blink(lblReceivedLED);
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                var lastLine = tBoxDataOut.Text.Split('\n').Last();
                DataOut = lastLine;
                serialPort1.Write(DataOut);
                Global.AppendToFile(String.Format("SENT :{0}", string.Join(" ", lastLine)));
                //tBoxDataOut.AppendText(Environment.NewLine);
                Global.Blink(lblSentLED);
            }
            else
            {
                MessageBox.Show("Port was not opened.", "Error");
            }
        }

        public void tBoxDataOut_TextChanged(object sender, EventArgs e)
        {
            int OutLength = tBoxDataOut.TextLength;
            lblDataOutLength.Text = string.Format("{0:00}", OutLength);
        }

        private void tBoxDataIN_TextChanged(object sender, EventArgs e)
        {
            int InLength = tBoxDataIN.TextLength;
            lblDataInLength.Text = string.Format("{0:00}", InLength);
        }

        private void saveAsToolStrip_Click(object sender, EventArgs e)
        {
            Global.SaveAs();
        }

        private void ConfiguratePortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Global.MultipleOpenForm("Manually Configurate") == false)
            {
                if(serialPort1.IsOpen)                    
                {
                    serialPort1.Close();
                    Global.AppendToFile(serialPort1.PortName + " was closed.");

                    btnClosePort.Enabled = false;
                    btnOpenPort.Enabled = true;
                }
                f3 = new Form3();
                f3.FormClosed += f3_FormClosed;
                f3.Show();
            }
        }

        private void f3_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                btnOpenPort.Enabled = false;
                btnClosePort.Enabled = true;
            }
        }

        private void btnOpenPort_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    btnOpenPort.Enabled = false;
                    btnClosePort.Enabled = true;

                    serialPort1.Open();
                    Global.AppendToFile(Global.GlobalVar.PortName + " was opened.");                    

                    MessageBox.Show("The configuration is: " + Global.GlobalVar.BaudRate + ", " + Global.GlobalVar.DataBits
                        + ", " + Global.GlobalVar.StopBits + ", " + Global.GlobalVar.Parity + ".", "Information");
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error in opening port", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnOpenPort.Enabled = true;
                    btnClosePort.Enabled = false;
                }
            }
            else
            {
                btnOpenPort.Enabled = false;
                btnClosePort.Enabled = true;
            }
        }

        private void btnClosePort_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                btnClosePort.Enabled = false;
                btnOpenPort.Enabled = true;

                serialPort1.Close();

                Global.AppendToFile(serialPort1.PortName + " was closed.");
            }
            else
            {
                btnClosePort.Enabled = false;
                btnOpenPort.Enabled = true;
            }
        }        

        private void clearScreenOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tBoxDataOut.Text != "") tBoxDataOut.Text = "";
        }

        private void clearScreenInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tBoxDataIN.Text != "") tBoxDataIN.Text = "";
        }
        
        private void tBoxDataOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSendData_Click(sender, e);
                tBoxDataIN.AppendText(Environment.NewLine);
                //tBoxDataIN.AppendText(Environment.NewLine);
            }
            else Global.KeyDown(sender, e, tBoxDataOut);
        }

        private void CBoxComport_SelectionChangeCommitted_1(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                btnClosePort.Enabled = false;
                btnOpenPort.Enabled = true;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                serialPort1.Close();
                Global.AppendToFile(serialPort1.PortName + " was closed.");
            }
        }

        private void viewInBinaryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetValueForTextOut = tBoxDataOut.Text;
            SetValueForTextIn = tBoxDataIN.Text;

            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                Global.AppendToFile(serialPort1.PortName + " was closed.");
                btnClosePort.Enabled = false;
                btnOpenPort.Enabled = true;
            }

            if (Global.MultipleOpenForm("Byte View") == false)
            {
                f4 = new Form4(this);
                f4.Show();
            }
        }

        private void viewInDecimalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetValueForTextOut = tBoxDataOut.Text;
            SetValueForTextIn = tBoxDataIN.Text;

            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                Global.AppendToFile(serialPort1.PortName + " was closed.");
                btnClosePort.Enabled = false;
                btnOpenPort.Enabled = true;
            }

            if (Global.MultipleOpenForm("Byte View") == false)
            {
                f2 = new Form2(this);
                f2.Show();
            }
        }
    }
}
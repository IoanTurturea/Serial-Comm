using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;

namespace SerialCommunication_C_Sharf_
{
    public partial class Form2 : Form
    {
        byte[] buffer;

        Form1 originalForm;
        public Form2(Form1 incomingForm)
        {
            originalForm = incomingForm;
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {            
            Global.OpenPort(serialPort1);
            if (serialPort1.IsOpen)
            {
                Global.AppendToFile(Global.GlobalVar.PortName + " was opened.");
                serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                btnContinue.Enabled = false;
                btnStop.Enabled = true;
            }
            else
            {
                btnStop.Enabled = false;
                btnContinue.Enabled = true;
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytes = serialPort1.BytesToRead;
                buffer = new byte[bytes];
                serialPort1.Read(buffer, 0, bytes);
                Global.AppendToFile(string.Format("RECEIVED {0}bytes: {1}", bytes, string.Join(" ", buffer)));
                Invoke(new EventHandler(ShowData));
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error in showing data on screen.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowData(object sender, EventArgs e)
        {
            tBoxDataIN.Text += string.Join(" ", buffer);
            tBoxDataIN.SelectionStart = tBoxDataIN.TextLength;
            tBoxDataIN.ScrollToCaret();
            tBoxDataIN.AppendText(Environment.NewLine);
            Global.Blink(lblReceivedLED);
        }

        private static byte[] GetByteArrayFromIntString(string input)
        {
            return input
            .Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(i => Convert.ToByte(i, 10))
            .ToArray();
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (tBoxDataOut.Text.Split('\n').Last() != "")
                {
                    try
                    { 
                    byte[] instruction = GetByteArrayFromIntString(tBoxDataOut.Text.Split('\n').Last());
                    serialPort1.Write(instruction, 0, instruction.Length);
                    tBoxDataOut.AppendText(Environment.NewLine);
                    Global.AppendToFile(String.Format("SENT: {0}", string.Join(" ", instruction)));
                    Global.Blink(lblSentLED);
                    }
                    catch
                    {
                        MessageBox.Show("Numbers may not be larger than 255(1 Byte).\n" +
                           "This data was not sent.","Error");
                    }
                }
                else MessageBox.Show("You may not send empty data.");
            }
            else MessageBox.Show("Port was not opened.");
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

        private void saveAs_Click(object sender, EventArgs e)
        {
            Global.SaveAs();
        }

        private void clearScreenOut_Click(object sender, EventArgs e)
        {
            if (tBoxDataOut.Text != "") tBoxDataOut.Text = "";
        }

        private void clearScreenIn_Click(object sender, EventArgs e)
        {
            if (tBoxDataIN.Text != "") tBoxDataIN.Text = "";
        }

        private void tBoxDataOut_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!((c >= '0' && c <= '9') || c == ' ' || c == ',' || c == ';' || c == (char)8))
            {
                e.Handled = true;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                Global.AppendToFile(Global.GlobalVar.PortName + " was closed.");
                btnContinue.Enabled = true;
                btnStop.Enabled = false;
            }
            else if (!serialPort1.IsOpen)
            {
                btnContinue.Enabled = true;
                btnContinue.Enabled = false;
            }
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    btnContinue.Enabled = false;
                    btnStop.Enabled = true;

                    serialPort1.Open();
                    Global.AppendToFile(Global.GlobalVar.PortName + " was opened.");                    

                    MessageBox.Show("The configuration is: " + Global.GlobalVar.BaudRate + ", " + Global.GlobalVar.DataBits
                        + ", " + Global.GlobalVar.StopBits + ", " + Global.GlobalVar.Parity + ".", "Information");
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error in opening port", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnContinue.Enabled = true;
                    btnStop.Enabled = false;
                }
            }
            else
            {
                btnContinue.Enabled = false;
                btnStop.Enabled = true;
            }
        }

        private void sentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Global.ConvertToDecimalFromString(sender, e, originalForm.tBoxDataOut.Text, tBoxDataOut);
        }

        private void receivedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Global.ConvertToDecimalFromString(sender, e, originalForm.tBoxDataIN.Text, tBoxDataIN);
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                Global.AppendToFile(Global.GlobalVar.PortName + " was closed.");
            }
        }
    }
}

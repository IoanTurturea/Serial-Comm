using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;

namespace SerialCommunication_C_Sharf_
{
    /// <summary>
    /// Main Form of the software
    /// Uses Byte Read from serial port and Hex view
    /// </summary>
    public partial class Form5 : Form
    {
        // represents computer buffer
        byte[] buffer; 

        Form1 f1; 
        Form3 f3;        

        public Form5(/*Form1 incomingForm*/) 
        {
            //originalForm = incomingForm;
            InitializeComponent();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            Global.PortName(CBoxComport);
            Global.GlobalVar.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            if(Global.GlobalVar.IsOpen)
            {
                Global.AppendToFile(Global.GlobalVar.PortName + " was opened.");
                btnContinue.Enabled = false;
                btnStop.Enabled = true;
            }
            else
            {
                btnContinue.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytes = Global.GlobalVar.BytesToRead;
                buffer = new byte[bytes];
                Global.GlobalVar.Read(buffer, 0, bytes); // argument list: array buffer, offset, count
                string temp = "";
                foreach(byte b in buffer)
                {
                    temp += b.ToString("X") + " ";
                }
                Global.AppendToFile(String.Format("RECEIVED {0}bytes: {1}", bytes , temp));
                Invoke(new EventHandler(ShowData));
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error in showing data on screen.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*
         *Show the received data from the handler
        */
        private void ShowData(object sender, EventArgs e)
        {
            string temp = "";
            foreach(byte b in buffer)
            {
                temp += b.ToString("X") + " ";
            }
            
            tBoxDataIN.Text += temp;
            tBoxDataIN.SelectionStart = tBoxDataIN.TextLength;
            tBoxDataIN.ScrollToCaret();
            Global.Blink(lblReceivedLED);
        }

        private static byte[] GetByteArrayFromHexString(string input)
        {
            return input
                .Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(i => Convert.ToByte(i, 16))
                .ToArray();
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            if (Global.GlobalVar.IsOpen)
            {
                if (tBoxDataOut.Text.Split('\n').Last() != "")
                {
                    try
                    {
                        byte[] instruction = GetByteArrayFromHexString(tBoxDataOut.Text.Split('\n').Last());
                        Global.GlobalVar.Write(instruction, 0, instruction.Length);
                        tBoxDataOut.AppendText(Environment.NewLine);
                        Global.AppendToFile(String.Format("SENT: {0}", string.Join(" ", instruction)));
                        Global.Blink(lblSentLED);

                    }
                    catch
                    {
                        MessageBox.Show("Numbers may not be larger than FF(1 Byte).\n" +
                        "This data was not sent.", "Error");
                    }
                }
                else MessageBox.Show("You may not send empty data.");
            }
            else MessageBox.Show("Port was not opened.");
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

        private void tBoxDataOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {                          
                btnSendData_Click(sender, e);
                tBoxDataIN.AppendText(Environment.NewLine);
            }                          
            else if (e.Control && e.KeyCode == Keys.M) 
            {
                e.SuppressKeyPress = true;
                f3.Show();
            }
            else Global.KeyDown(sender, e, tBoxDataOut);            
        }

        // input in Hex
        private void tBoxDataOut_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f') || c == ' ' || c == ',' || c == ';' || c == (char)8))
            {
                e.Handled = true;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if(Global.GlobalVar.IsOpen)
            {
                Global.GlobalVar.Close();
                Global.AppendToFile(Global.GlobalVar.PortName + " was closed.");
                btnContinue.Enabled = true;
                btnStop.Enabled = false;
            }
            else if (!Global.GlobalVar.IsOpen)
            {
                btnContinue.Enabled = true;
                btnContinue.Enabled = false;
            }
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            if (!Global.GlobalVar.IsOpen)
            {
                btnContinue.Enabled = false;
                btnStop.Enabled = true;
                try
                {
                    Global.GlobalVar.PortName = CBoxComport.Text;

                    Global.OpenPort(Global.GlobalVar);

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

        private void Form5_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Global.GlobalVar.IsOpen)
            {
                Global.GlobalVar.Close();
                Global.AppendToFile(Global.GlobalVar.PortName + " was closed.");
            }
        }

        // open settings == Form3. 
        private void manuallyConfiguratePortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Global.MultipleOpenForm("Manually Configurate") == false)
            {
                if (Global.GlobalVar.IsOpen)
                {
                    Global.GlobalVar.Close();
                    Global.AppendToFile(Global.GlobalVar.PortName + " was closed.");

                    btnStop.Enabled = false;
                    btnContinue.Enabled = true;
                }
                f3 = new Form3();
                f3.FormClosed += f3_FormClosed;
                f3.Show();
            }
        }

        private void f3_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Global.GlobalVar.IsOpen)
            {
                btnStop.Enabled = true;
                btnContinue.Enabled = false;
            }
        }
        
        //  open form1
        private void viewInStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Global.GlobalVar.IsOpen)
            {
                Global.GlobalVar.Close();
                Global.AppendToFile(Global.GlobalVar.PortName + " was closed.");
                btnStop.Enabled = false;
                btnContinue.Enabled = true;
            }

            if (Global.MultipleOpenForm("String View") == false)
            {
                f1 = new Form1();
                f1.Show();
            }
        }

        private void CBoxComport_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (Global.GlobalVar.IsOpen)
            {
                Global.GlobalVar.Close();
                btnStop.Enabled = false;
                btnContinue.Enabled = true;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Form5_Load(sender, e);
        }

        private void Form5_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult dialog = MessageBox.Show("Are you sure you want to close?", "Exit", buttons);
            if (dialog == DialogResult.Yes)
            {
                if (Global.GlobalVar.IsOpen)
                {
                    Global.GlobalVar.Close();
                    Global.AppendToFile(Global.GlobalVar.PortName + " was closed.");
                }
                e.Cancel = false;
            }
            else if (dialog == DialogResult.No) e.Cancel = true;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        }
    }
}

using System;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Drawing; 

namespace SerialCommunication_C_Sharf_
{
    /// <summary>
    /// Public class that contains all function to be used in Forms
    /// in order to avoid code duplicate
    /// </summary>
    static class Global
    {
        // the serial port to be configured from Form5 and From3 (only)
        private static SerialPort serialPort = new SerialPort();

        public static SerialPort GlobalVar
        {
            get { return serialPort; }
            set { serialPort = value; }
        }

        // saves the session in a Monitor.txt file
        public static void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt  | Word Document|*.doc";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;
            saveFileDialog.ShowDialog();
            string myFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Monitor.txt");

            try
            {
                if (saveFileDialog.FileName.Contains(".doc") || saveFileDialog.FileName.Contains(".txt"))
                {
                    string content = File.ReadAllText(myFilePath);
                    File.WriteAllText(saveFileDialog.FileName, content);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        // creates log file
        public static void AppendToFile(string toAppend)
        {
            string myFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Monitor.txt");
            string time = DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss:FFF");
            File.AppendAllText(myFilePath, time + " " + toAppend + Environment.NewLine);
        }

        public static void OpenPort(SerialPort serialPort)
        {
            try
            {
                serialPort.PortName  = Global.GlobalVar.PortName;
                serialPort.BaudRate  = Global.GlobalVar.BaudRate;
                serialPort.Parity    = Global.GlobalVar.Parity;
                serialPort.DataBits  = Global.GlobalVar.DataBits;
                serialPort.StopBits  = Global.GlobalVar.StopBits;
                serialPort.Handshake = Global.GlobalVar.Handshake;
                serialPort.DtrEnable = Global.GlobalVar.DtrEnable;
                serialPort.RtsEnable = Global.GlobalVar.RtsEnable;

                serialPort.Open();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error in opening port", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /* 
         * Super-keys function
         */ 
        public static void KeyDown(object sender, KeyEventArgs e, TextBox tBoxDataOut)
        {
            if (e.KeyCode == Keys.Back)
            {
                if (tBoxDataOut.Text != "")
                {
                    tBoxDataOut.Text = tBoxDataOut.Text.Remove(tBoxDataOut.Text.Length-1, 0);
                    tBoxDataOut.SelectionStart = tBoxDataOut.Text.Length;
                    tBoxDataOut.SelectionLength = 0;
                }
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                e.SuppressKeyPress = true;
                tBoxDataOut.Copy();
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                e.SuppressKeyPress = true;
                tBoxDataOut.Paste();
            }
            else if (e.Control && e.KeyCode == Keys.X)
            {
                e.SuppressKeyPress = true;
                tBoxDataOut.Cut();
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                tBoxDataOut.SelectAll();
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                SaveAs();
            }
            else if (e.Control && e.KeyCode == Keys.Z)
            {
                e.SuppressKeyPress = true;
                tBoxDataOut.Undo();
            }
        }

        // fill comboBox with available Port names
        public static void PortName(ComboBox CBoxComport)
        {
            CBoxComport.Items.Clear();
            CBoxComport.Text = "";

            string[] ports = SerialPort.GetPortNames();

            for (int i = 0; i < ports.Length; ++i)
            {
                if (!CBoxComport.Items.Contains(ports[i])) CBoxComport.Items.AddRange(ports);
            }
            if (ports.Length != 0) CBoxComport.Text = ports[0];
        }

        /* 
         * Invert receive bits
         */ 
        public static string InvertCorect(string de_inversat)
        {
            char[] chars = de_inversat.ToCharArray();
            string new_chars = "";

            for (int i = 0; i < chars.Length; ++i)
            {
                if (chars[i] == ' ')
                {
                    for (int ii = i; ii > i - 9; --ii)
                    {
                        new_chars += chars[ii].ToString();
                    }
                }
            }
            return new_chars;
        }

        public static void ConvertToBinaryFromString(object sender, EventArgs e, TextBox tBoxData, string TextIn)
        {
            byte[] array = Encoding.ASCII.GetBytes(TextIn);
            var bits = new BitArray(array);
            int count = 0;
            string ceva = "";

            foreach (bool b in bits)
            {
                ++count;
                ceva += Convert.ToInt32(b).ToString();
                if (count % 8 == 0) ceva += " ";
            }

            tBoxData.Text += InvertCorect(ceva);
            tBoxData.Text = tBoxData.Text.Replace("00001010 00001101 ", "\r\n").Replace(" 00100000", " ");
        }

        public static string ConvertToBinaryFromByte(object sender, EventArgs e, byte[] buffer)
        {
            var bits = new BitArray(buffer);
            string ceva = "";
            int count = 0;
            foreach (bool b in bits)
            {
                count++;
                ceva += Convert.ToInt32(b).ToString();
                if (count % 8 == 0) ceva += " ";
            }

            return InvertCorect(ceva);
        }

        public static void ConvertToDecimalFromString(object sender, EventArgs e, string TextIn, TextBox tBoxData)
        {
            byte[] array = Encoding.ASCII.GetBytes(TextIn);
            foreach (byte element in array)
            {
                tBoxData.Text += element + " ";
            }
            tBoxData.Text = tBoxData.Text.Replace("13 10", "\r\n").Replace(" 32"," ");
        }

        public static string ExtractBetween(string s, int commaPosition)
        {
            char[] c = s.ToCharArray(); 
            // not the smartest
            char[] noulc = new char[40] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',' ', ' ',
                                          ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };

            string temp = "";
            int count = 0;
            for (int i = 0; i < c.Length; ++i)
            {
                if (c[i] == ';') ++count;
                if (count == commaPosition)
                {
                    noulc[i + 1] = c[i + 1];
                    if (noulc[i + 2] == ';')
                    {
                        goto aici;
                    }
                }
            }
            aici:
            foreach (char cr in noulc)
            {
                if (!(cr == ' ' || cr == ';'))
                    temp += cr;
            }
            return temp;
        }

        // set communcation params
        public static void SetPort(ComboBox CBoxComport, int BaudRate, int DataBits, StopBits stopBits, 
            Parity parity, Handshake handshake, bool DTR, bool RTS)
        {
            Global.GlobalVar.PortName = CBoxComport.Text;
            Global.GlobalVar.BaudRate = BaudRate;
            Global.GlobalVar.DataBits = DataBits;
            Global.GlobalVar.StopBits = stopBits;
            Global.GlobalVar.Parity = parity;
            Global.GlobalVar.Handshake = handshake;
            Global.GlobalVar.DtrEnable = DTR;
            Global.GlobalVar.RtsEnable = RTS;
            Global.GlobalVar.Open();

            Global.AppendToFile(Global.GlobalVar.PortName + " was opened. "  // aici e o eroare
           + "Configuration: " + Global.GlobalVar.BaudRate + ", " + Global.GlobalVar.DataBits
           + ", " + Global.GlobalVar.StopBits + ", " + Global.GlobalVar.Parity
           + ", " + "DTR: " + Global.GlobalVar.DtrEnable + ", " + "RTS: " + Global.GlobalVar.RtsEnable
           + ", " + "Handshake: " + Global.GlobalVar.Handshake.ToString());
        }

        /*
         * blink LED on Rx/Tx. runs on Thread
         */ 
        public static async void Blink(Label label1)
        {
            await Task.Delay(10);
                label1.BackColor = Color.Red;
            await Task.Delay(10);
                label1.BackColor = Color.White;
        }


        //is a Form alredy opened?         
        public static bool MultipleOpenForm(string name)
        {
            bool isOpen = false;

            foreach (Form f in Application.OpenForms)
            {
                if (f.Text == name)
                {
                    isOpen = true;
                    f.BringToFront();
                    break;
                }
            }
            return isOpen;
        }

        // IEEE Floating Point Converter 32 Bit
        // if needed is here
        public static double FromFloatSafe(object f) 
        {
            uint fb = Convert.ToUInt32(f);
            return BitConverter.ToSingle(BitConverter.GetBytes((int)fb), 0);
        }
    }
}

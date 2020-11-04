using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;


namespace SerialCommunication_C_Sharf_
{
    public partial class Form3 : Form
    {
        private Dictionary<int, string> map = new Dictionary<int, string>();
        private string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"configurations.txt");

        const string v = ";";

        public Form3()
        {
            InitializeComponent();
            FillCombo();
        }

        private void FillCombo()
        {
            if(File.Exists(filePath))
            {
                try
                {
                    var TextLines = File.ReadAllLines(filePath);
                    
                    for (int i = 0; i < TextLines.Length; ++i)
                    {
                        cmbBoxSavedConfigurations.Items.Add(Before(TextLines[i],v));
                        map.Add(i, Before(TextLines[i], v));                        
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }            
        }

        public string Before(string value, string a)
        {
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Global.PortName(CBoxComport);

            chBoxDRTEnable.Checked = false;
            Global.GlobalVar.DtrEnable = false;
            chBoxRTSEnable.Checked = false;
            Global.GlobalVar.RtsEnable = false;

            btnClose.Enabled = false;
            btnOpen.Enabled = true;
        }       

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!Global.GlobalVar.IsOpen)
            {                
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                try
                {
                    Global.SetPort(CBoxComport, Convert.ToInt32(CBoxBaudRate.Text), Convert.ToInt32(CBoxDataBits.Text),
                        (StopBits)Enum.Parse(typeof(StopBits), CBoxStopBits.Text), 
                        (Parity)Enum.Parse(typeof(Parity), CBoxParity.Text),
                        Global.GlobalVar.Handshake, Global.GlobalVar.DtrEnable, Global.GlobalVar.RtsEnable);

                    progressBar1.Value = 100;

                    btnOpen.Enabled = false;
                    btnClose.Enabled = true;

                    MessageBox.Show("The active configuration is: " + Global.GlobalVar.BaudRate + ", " + Global.GlobalVar.DataBits
                    + ", " + Global.GlobalVar.StopBits + ", " + Global.GlobalVar.Parity + ".", "Information");
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error in opening port", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    btnOpen.Enabled = true;
                    btnClose.Enabled = false;
                }
            }
            else 
            {
                btnOpen.Enabled = false;
                btnClose.Enabled = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (Global.GlobalVar.IsOpen)
            { 
                btnOpen.Enabled = true;
                btnClose.Enabled = false;

                Global.GlobalVar.Close();
                Global.AppendToFile(Global.GlobalVar.PortName + " was closed.");
                progressBar1.Value = 0;
            }
            else
            {
                btnClose.Enabled = false;
                btnOpen.Enabled = true;
            }
        }        
        
        private void chBoxDRTEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chBoxDRTEnable.Checked) Global.GlobalVar.DtrEnable = true;
                else Global.GlobalVar.DtrEnable = false;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error, DRT could nou be activated.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                chBoxDRTEnable.Checked = false;
            }
        }

        private void chBoxRTSEnable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chBoxRTSEnable.Checked) Global.GlobalVar.RtsEnable = true;
                else Global.GlobalVar.RtsEnable = false;
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message, "Error, RTS could nou be activated.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                chBoxRTSEnable.Checked = false;
            }
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            if (cmbBoxSavedConfigurations.Text != "" && CBoxBaudRate.Text != "" && CBoxDataBits.Text != ""
                && CBoxStopBits.Text != "" && CBoxParity.Text != "" && !cmbBoxSavedConfigurations.Items.Contains(cmbBoxSavedConfigurations.Text))
            {
                string config = cmbBoxSavedConfigurations.Text + v + CBoxBaudRate.Text + v + CBoxDataBits.Text + v +
                               CBoxStopBits.Text + v + CBoxParity.Text + ";\r\n";
                File.AppendAllText(filePath, config);
                cmbBoxSavedConfigurations.Items.Add(cmbBoxSavedConfigurations.Text);
                MessageBox.Show("Saved");
            }
            else if (cmbBoxSavedConfigurations.Items.Contains(cmbBoxSavedConfigurations.Text))
            {
                var TextLines = File.ReadAllLines(filePath);
                var myKey = map.FirstOrDefault(x => x.Value == cmbBoxSavedConfigurations.SelectedItem.ToString()).Key;
                if (cmbBoxSavedConfigurations.Items.Contains(cmbBoxSavedConfigurations.Text) &&
                    (CBoxBaudRate.Text == Global.ExtractBetween(TextLines[myKey], 1)) &&
                    (CBoxDataBits.Text == Global.ExtractBetween(TextLines[myKey], 2)) &&
                    (CBoxStopBits.Text == Global.ExtractBetween(TextLines[myKey], 3)) &&
                    (CBoxParity.Text == Global.ExtractBetween(TextLines[myKey], 4)))
                    MessageBox.Show("You may not duplicate configurations names.", "Error");
                else
                {
                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    DialogResult dialog = MessageBox.Show("Do you want to change this configuration?", "Modify", buttons);
                    if (dialog == DialogResult.Yes)
                    {
                        string config = cmbBoxSavedConfigurations.Text + v + CBoxBaudRate.Text + v + CBoxDataBits.Text + v +
                                    CBoxStopBits.Text + v + CBoxParity.Text + v + "\r\n";

                        string removeme = cmbBoxSavedConfigurations.SelectedItem.ToString();
                        var newLines = TextLines.Where(line => !line.Contains(removeme + v));

                        File.WriteAllLines(filePath, newLines);
                        File.AppendAllText(filePath, config);

                        MessageBox.Show("Changed", "Change Configuration");
                    }
                    else if (dialog == DialogResult.No)
                    {
                        dialog = DialogResult.No;
                    }
                }
            } 
            else
            {
               MessageBox.Show("You may not save empty data.", "Error");
            }
        }

        private void btnDeleteConfig_Click(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                if (cmbBoxSavedConfigurations.Items.Contains(cmbBoxSavedConfigurations.Text))
                {
                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    DialogResult dialog = MessageBox.Show("This configuration will be deleted.\n Do you want to continue?", "Delete Configuration", buttons);
                    if (dialog == DialogResult.Yes)
                    {
                        try
                        {
                            var TextLines = File.ReadAllLines(filePath);

                            //var myKey = map.FirstOrDefault(x => x.Value == cmbBoxSavedConfigurations.SelectedItem.ToString()).Key;
                            string removeme = cmbBoxSavedConfigurations.SelectedItem.ToString();

                            var newLines = TextLines.Where(line => !line.Contains(removeme + v));

                            File.WriteAllLines(filePath, newLines);

                            MessageBox.Show("Deleted", "Delete Configuration");

                            cmbBoxSavedConfigurations.Items.Remove(cmbBoxSavedConfigurations.SelectedItem);
                            cmbBoxSavedConfigurations.Text = "";
                            CBoxBaudRate.Text = "";
                            CBoxDataBits.Text = "";
                            CBoxStopBits.Text = "";
                            CBoxParity.Text = "";
                            Form3_Load(sender, e);                            
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show(err.Message);
                        }
                    }
                    else if (dialog == DialogResult.No)
                    {
                        dialog = DialogResult.No;
                    }
                }
                else
                {
                    MessageBox.Show("Name does not exist in the list.", "Error");
                }
            }
        }

        private void cmbBoxSavedConfigurations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    if (map.ContainsValue(cmbBoxSavedConfigurations.SelectedItem.ToString()))
                    {
                        var TextLines = File.ReadAllLines(filePath);
                        var myKey = map.FirstOrDefault(x => x.Value == cmbBoxSavedConfigurations.SelectedItem.ToString()).Key;
                        CBoxBaudRate.Text = Global.ExtractBetween(TextLines[myKey], 1);
                        CBoxDataBits.Text = Global.ExtractBetween(TextLines[myKey], 2);
                        CBoxStopBits.Text = Global.ExtractBetween(TextLines[myKey], 3);
                        CBoxParity.Text = Global.ExtractBetween(TextLines[myKey], 4);
                    }

                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }

        private void chBoxHandShake_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxHandShake.Checked)
            {
                chBoxHandShake.Enabled = true;
                Global.GlobalVar.Handshake = Handshake.XOnXOff;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Form3_Load(sender, e);
        }

        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult dialog = MessageBox.Show("All configurations will be deleted.\n Do you want to continue?", "Delete Configurations", buttons);
                if (dialog == DialogResult.Yes)
                {
                    File.Delete(filePath);
                    cmbBoxSavedConfigurations.Items.Clear();
                    cmbBoxSavedConfigurations.Text = "";
                    CBoxBaudRate.Text = "";
                    CBoxDataBits.Text = "";
                    CBoxStopBits.Text = "";
                    CBoxParity.Text = "";
                    MessageBox.Show("All configurations have be deleted.\n Deleted");
                }
                else if (dialog == DialogResult.No)
                {
                    Form3_Load(sender, e);
                }
            }
            else
            {
                MessageBox.Show("Nothig to delete.", "No configurations");
            }
        }

        private void cmbBoxSavedConfigurations_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (c == ' ' || c == ';')
            {
                e.Handled = true;
            }
        }

        private void CBoxComport_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Global.GlobalVar.IsOpen)
            {
                Global.GlobalVar.Close();
                btnClose.Enabled = false;
                btnOpen.Enabled = true;
                progressBar1.Value = 0;
            }                 
        }
    }
}
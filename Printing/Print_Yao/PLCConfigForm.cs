using NJCH_Station;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JY_Print
{
    public partial class PLCConfigForm : Form
    {
        public string SelectedIP { get; private set; }
        public int SelectedPort { get; private set; }

        string plcIP = Config.GetPLCIP();
        int plcPort = Config.GetPLCPort();
        public PLCConfigForm()
        {
            InitializeComponent();
            txtIP.Text = plcIP;
            txtPort.Text = plcPort.ToString();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtPort.Text, out int port) && port > 0 && port <= 65535)
            {
                SelectedIP = txtIP.Text.Trim();
                SelectedPort = port;

                // 保存配置到INI文件
                Config.SavePLCIP(SelectedIP);
                Config.SavePLCPort(SelectedPort);

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("端口号必须为1-65535之间的整数", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

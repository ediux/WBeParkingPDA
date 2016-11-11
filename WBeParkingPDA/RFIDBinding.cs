using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WBeParkingPDA
{
    public partial class RFIDBinding : BaseForm
    {
        private RFIDScanner rfidscanner;

        public RFIDBinding()
        {
            InitializeComponent();

            label4.Visible = false;
            rfidscanner = new RFIDScanner(this);
            rfidscanner.TagInputBox = textBox1;
            rfidscanner.ScannerStatusLabel = label4;
        }

        private void RFIDBinding_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            rfidscanner.EnableReader();
        }

        private void RFIDBinding_Closing(object sender, CancelEventArgs e)
        {
            rfidscanner.DisableReader();
        }

        private void btnBackTo_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
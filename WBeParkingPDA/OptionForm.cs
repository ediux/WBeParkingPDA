using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WBeParkingPDA
{
    public partial class OptionForm : WBeParkingPDA.BaseForm
    {
        public OptionForm()
        {
            InitializeComponent();
            memdb = SyncDataViewModel.LoadFile(Utility.jsondbpath);
        }

        private SyncDataViewModel memdb;

        private void OptionForm_Load(object sender, EventArgs e)
        {
            tbServerIP.Text = memdb.AppSettings["RemoteHost"] as string;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            memdb.AppSettings["RemoteHost"] = tbServerIP.Text;
            SyncDataViewModel.SaveFile(Utility.jsondbpath, memdb);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = btnCancel.DialogResult;
            this.Close();
        }
    }
}


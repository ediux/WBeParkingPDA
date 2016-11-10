using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WBeParkingPDA
{
    public partial class Main : WBeParkingPDA.BaseForm
    {
        private BackgroundWorker bgworker;

        public Main() :base()
        {
            InitializeComponent();
            
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Utility.HideTaskbar();
        }

        private void Main_Closing(object sender, CancelEventArgs e)
        {
            Utility.ShowTaskbar();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == System.Windows.Forms.Keys.Up))
            {
                // Up
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Down))
            {
                // Down
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Left))
            {
                // Left
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Right))
            {
                // Right
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Enter))
            {
                // Enter
            }

        }

        private void btnRFIDBinding_Click(object sender, EventArgs e)
        {

            RFIDBinding rfidBindingForm = new RFIDBinding();
            rfidBindingForm.Show();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (panelUploadStatus.Visible == false)
            {
                panelUploadStatus.Visible = true;
            }
            else
            {
                panelUploadStatus.Visible = false;
            }
        }
    }
}


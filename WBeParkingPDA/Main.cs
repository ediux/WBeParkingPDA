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

        public Main()
            : base()
        {
            InitializeComponent();

            //初始化非同步背景作業
            bgworker = new BackgroundWorker(this);
            bgworker.DoWork += new DoWorkEventHandler(bgworker_DoWork);
            bgworker.ProgressChanged += new ProgressChangedEventHandler(bgworker_ProgressChanged);
            bgworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgworker_RunWorkerCompleted);
            bgworker.WorkerReportsProgress = true;
            bgworker.WorkerSupportsCancellation = false;
        }

        private void bgworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void bgworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
           
        }

        private void bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            
        }

        private void Main_Load(object sender, EventArgs e)
        {
            CoreDLL.HideTaskbar();
        }

        private void Main_Closing(object sender, CancelEventArgs e)
        {
            CoreDLL.ShowTaskbar();
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

        private void menuItem_AppExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}


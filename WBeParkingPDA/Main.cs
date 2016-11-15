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

            progressBar1.Maximum = 100;
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
        }

        private void bgworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            panelUploadStatus.Visible = false;
            btnUpload.Enabled = true;
            if (e.Result is bool)
            {
                if (((bool)e.Result))
                {
                    MessageBox.Show("同步成功!");
                }
                else
                {
                    MessageBox.Show("同步失敗!");
                }
            }
        }

        private void bgworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            if (e.UserState is Exception)
            {
                labelUploadStatus.Text = "傳送失敗!!"+((Exception)e.UserState).Message;
            }
            else
            {
                labelUploadStatus.Text = string.Format("{0}%", e.ProgressPercentage);
            }
        }

        private void bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            bgworker.ReportProgress(10);
            WBeParkingPDA.Classes.WebClientHelper client = new WBeParkingPDA.Classes.WebClientHelper();
            bgworker.ReportProgress(50);
            try
            {
                SyncDataViewModel result = client.PostData(Utility.GetDBJson(), "http://192.168.2.80:5002/api/SQLiteSync");
            }
            catch (Exception ex)
            {
                bgworker.ReportProgress(50, ex);
               
                e.Result = false;
                return;
            }

            bgworker.ReportProgress(99);

            bgworker.ReportProgress(100);
            e.Result = true;

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
                bgworker.RunWorkerAsync();
                btnUpload.Enabled = false;
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


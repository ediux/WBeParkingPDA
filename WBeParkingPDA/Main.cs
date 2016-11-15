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
                    Utility.ShowInfoMsg("同步成功!");
                }
                else
                {
                    Utility.ShowErrMsg("同步失敗!");
                }
            }
        }

        private void bgworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            if (e.UserState is Exception)
            {
                labelUploadStatus.Text = "傳送失敗!!" + ((Exception)e.UserState).Message;
            }
            else
            {
                if (e.UserState is string)
                {
                    labelUploadStatus.Text = string.Format("{0},{1}%", e.UserState,e.ProgressPercentage);
                }
                else
                {
                    labelUploadStatus.Text = string.Format("{0}%", e.ProgressPercentage);
                }
                
            }
        }

        private void bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            bgworker.ReportProgress(0,"準備同步...");
            WBeParkingPDA.Classes.WebClientHelper client = new WBeParkingPDA.Classes.WebClientHelper();            
            try
            {
                bgworker.ReportProgress(30, "準備同步...");
                SyncDataViewModel source = SyncDataViewModel.LoadFile(Utility.jsondbpath);
                bgworker.ReportProgress(50, "連線中...");
                SyncDataViewModel clonemem = source.CloneToUploadSync();
                bgworker.ReportProgress(75, "上載資料中...");
                clonemem = client.PostData(clonemem, (source.AppSettings["RemoteHost"] as string) + "/api/SQLiteSync");
                bgworker.ReportProgress(90, "下載資料並處理...");
                clonemem.AppSettings = source.AppSettings;
                bgworker.ReportProgress(97, "下載資料並處理...");
                SyncDataViewModel.SaveFile(Utility.jsondbpath, clonemem);
                bgworker.ReportProgress(99, "同步即將完成!");
            }
            catch (Exception ex)
            {
                bgworker.ReportProgress(75, ex);

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


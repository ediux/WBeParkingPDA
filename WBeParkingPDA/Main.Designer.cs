namespace WBeParkingPDA
{
    partial class Main
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.btnRFIDBinding = new System.Windows.Forms.Button();
            this.panelUploadStatus = new System.Windows.Forms.Panel();
            this.labelUploadStatus = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnUpload = new System.Windows.Forms.Button();
            this.sys_contextmenu = new System.Windows.Forms.ContextMenu();
            this.menuItem_AppExit = new System.Windows.Forms.MenuItem();
            this.panelUploadStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRFIDBinding
            // 
            this.btnRFIDBinding.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRFIDBinding.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.btnRFIDBinding.Location = new System.Drawing.Point(51, 109);
            this.btnRFIDBinding.Name = "btnRFIDBinding";
            this.btnRFIDBinding.Size = new System.Drawing.Size(387, 54);
            this.btnRFIDBinding.TabIndex = 0;
            this.btnRFIDBinding.Text = "綁定";
            this.btnRFIDBinding.Click += new System.EventHandler(this.btnRFIDBinding_Click);
            // 
            // panelUploadStatus
            // 
            this.panelUploadStatus.BackColor = System.Drawing.Color.SkyBlue;
            this.panelUploadStatus.Controls.Add(this.labelUploadStatus);
            this.panelUploadStatus.Controls.Add(this.progressBar1);
            this.panelUploadStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelUploadStatus.Location = new System.Drawing.Point(0, 700);
            this.panelUploadStatus.Name = "panelUploadStatus";
            this.panelUploadStatus.Size = new System.Drawing.Size(480, 100);
            this.panelUploadStatus.Visible = false;
            // 
            // labelUploadStatus
            // 
            this.labelUploadStatus.BackColor = System.Drawing.Color.SkyBlue;
            this.labelUploadStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelUploadStatus.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Regular);
            this.labelUploadStatus.Location = new System.Drawing.Point(0, 52);
            this.labelUploadStatus.Name = "labelUploadStatus";
            this.labelUploadStatus.Size = new System.Drawing.Size(480, 48);
            this.labelUploadStatus.Text = "label1";
            this.labelUploadStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(3, 3);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(474, 42);
            // 
            // btnUpload
            // 
            this.btnUpload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpload.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.btnUpload.Location = new System.Drawing.Point(51, 169);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(387, 54);
            this.btnUpload.TabIndex = 1;
            this.btnUpload.Text = "上傳";
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // sys_contextmenu
            // 
            this.sys_contextmenu.MenuItems.Add(this.menuItem_AppExit);
            // 
            // menuItem_AppExit
            // 
            this.menuItem_AppExit.Text = "結束程式";
            this.menuItem_AppExit.Click += new System.EventHandler(this.menuItem_AppExit_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(480, 800);
            this.ContextMenu = this.sys_contextmenu;
            this.ControlBox = false;
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.panelUploadStatus);
            this.Controls.Add(this.btnRFIDBinding);
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "Main";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Main_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Main_Closing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.Controls.SetChildIndex(this.btnRFIDBinding, 0);
            this.Controls.SetChildIndex(this.panelUploadStatus, 0);
            this.Controls.SetChildIndex(this.btnUpload, 0);
            this.panelUploadStatus.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRFIDBinding;
        private System.Windows.Forms.Panel panelUploadStatus;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelUploadStatus;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.ContextMenu sys_contextmenu;
        private System.Windows.Forms.MenuItem menuItem_AppExit;
    }
}

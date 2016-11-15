namespace WBeParkingPDA
{
    partial class RFIDBinding
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
            this.btnBackTo = new System.Windows.Forms.Button();
            this.lbl_eTag = new System.Windows.Forms.Label();
            this.tb_eTagEPC = new System.Windows.Forms.TextBox();
            this.tbCarId = new System.Windows.Forms.TextBox();
            this.lblCarID = new System.Windows.Forms.Label();
            this.lblPurposeTypes = new System.Windows.Forms.Label();
            this.ddlPurposeTypes = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnBackTo
            // 
            this.btnBackTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBackTo.Font = new System.Drawing.Font("Tahoma", 22F, System.Drawing.FontStyle.Bold);
            this.btnBackTo.Location = new System.Drawing.Point(3, 759);
            this.btnBackTo.Name = "btnBackTo";
            this.btnBackTo.Size = new System.Drawing.Size(161, 38);
            this.btnBackTo.TabIndex = 0;
            this.btnBackTo.Text = "回上一頁";
            this.btnBackTo.Click += new System.EventHandler(this.btnBackTo_Click);
            // 
            // lbl_eTag
            // 
            this.lbl_eTag.Font = new System.Drawing.Font("Tahoma", 22F, System.Drawing.FontStyle.Regular);
            this.lbl_eTag.Location = new System.Drawing.Point(0, 83);
            this.lbl_eTag.Name = "lbl_eTag";
            this.lbl_eTag.Size = new System.Drawing.Size(77, 42);
            this.lbl_eTag.Text = "eTag";
            // 
            // tb_eTagEPC
            // 
            this.tb_eTagEPC.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tb_eTagEPC.Enabled = false;
            this.tb_eTagEPC.Font = new System.Drawing.Font("Tahoma", 22F, System.Drawing.FontStyle.Regular);
            this.tb_eTagEPC.Location = new System.Drawing.Point(79, 83);
            this.tb_eTagEPC.Name = "tb_eTagEPC";
            this.tb_eTagEPC.ReadOnly = true;
            this.tb_eTagEPC.Size = new System.Drawing.Size(398, 42);
            this.tb_eTagEPC.TabIndex = 2;
            // 
            // tbCarId
            // 
            this.tbCarId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCarId.Font = new System.Drawing.Font("Tahoma", 22F, System.Drawing.FontStyle.Regular);
            this.tbCarId.Location = new System.Drawing.Point(79, 131);
            this.tbCarId.Name = "tbCarId";
            this.tbCarId.Size = new System.Drawing.Size(398, 42);
            this.tbCarId.TabIndex = 4;
            this.tbCarId.GotFocus += new System.EventHandler(this.tbCarId_GotFocus);
            this.tbCarId.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox2_KeyUp);
            // 
            // lblCarID
            // 
            this.lblCarID.Font = new System.Drawing.Font("Tahoma", 22F, System.Drawing.FontStyle.Regular);
            this.lblCarID.Location = new System.Drawing.Point(0, 131);
            this.lblCarID.Name = "lblCarID";
            this.lblCarID.Size = new System.Drawing.Size(77, 42);
            this.lblCarID.Text = "車號";
            // 
            // lblPurposeTypes
            // 
            this.lblPurposeTypes.Font = new System.Drawing.Font("Tahoma", 22F, System.Drawing.FontStyle.Regular);
            this.lblPurposeTypes.Location = new System.Drawing.Point(3, 179);
            this.lblPurposeTypes.Name = "lblPurposeTypes";
            this.lblPurposeTypes.Size = new System.Drawing.Size(77, 42);
            this.lblPurposeTypes.Text = "用途";
            // 
            // ddlPurposeTypes
            // 
            this.ddlPurposeTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlPurposeTypes.Font = new System.Drawing.Font("Tahoma", 22F, System.Drawing.FontStyle.Regular);
            this.ddlPurposeTypes.Location = new System.Drawing.Point(79, 179);
            this.ddlPurposeTypes.Name = "ddlPurposeTypes";
            this.ddlPurposeTypes.Size = new System.Drawing.Size(398, 43);
            this.ddlPurposeTypes.TabIndex = 8;
            this.ddlPurposeTypes.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ddlPurposeTypes_KeyUp);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Enabled = false;
            this.btnSave.Font = new System.Drawing.Font("Tahoma", 22F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(373, 228);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(104, 40);
            this.btnSave.TabIndex = 13;
            this.btnSave.Text = "儲存";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // RFIDBinding
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(480, 800);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.ddlPurposeTypes);
            this.Controls.Add(this.lblPurposeTypes);
            this.Controls.Add(this.lblCarID);
            this.Controls.Add(this.tbCarId);
            this.Controls.Add(this.tb_eTagEPC);
            this.Controls.Add(this.lbl_eTag);
            this.Controls.Add(this.btnBackTo);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "RFIDBinding";
            this.Text = "RFIDBinding";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.RFIDBinding_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.RFIDBinding_Closing);
            this.Controls.SetChildIndex(this.btnBackTo, 0);
            this.Controls.SetChildIndex(this.lbl_eTag, 0);
            this.Controls.SetChildIndex(this.tb_eTagEPC, 0);
            this.Controls.SetChildIndex(this.tbCarId, 0);
            this.Controls.SetChildIndex(this.lblCarID, 0);
            this.Controls.SetChildIndex(this.lblPurposeTypes, 0);
            this.Controls.SetChildIndex(this.ddlPurposeTypes, 0);
            this.Controls.SetChildIndex(this.btnSave, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnBackTo;
        private System.Windows.Forms.Label lbl_eTag;
        private System.Windows.Forms.TextBox tb_eTagEPC;
        private System.Windows.Forms.TextBox tbCarId;
        private System.Windows.Forms.Label lblCarID;
        private System.Windows.Forms.Label lblPurposeTypes;
        private System.Windows.Forms.ComboBox ddlPurposeTypes;
        private System.Windows.Forms.Button btnSave;

    }
}
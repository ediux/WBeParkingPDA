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
        static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(RFIDBinding));

        private RFIDScanner rfidscanner;

        public RFIDBinding()
        {
            try
            {

                InitializeComponent();


                rfidscanner = new RFIDScanner(this);
                rfidscanner.TagInputBox = tb_eTagEPC;
                rfidscanner.OnAfterTagRead += new EventHandler<ThingMagic.TagReadDataEventArgs>(rfidscanner_OnAfterTagRead);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }

        void rfidscanner_OnAfterTagRead(object sender, ThingMagic.TagReadDataEventArgs e)
        {
            try
            {

                NextFocus(tbCarId);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

        }

        private void RFIDBinding_Load(object sender, EventArgs e)
        {
            try
            {
                this.WindowState = FormWindowState.Maximized;

                //先初始化資料庫
                Utility.Initializedatabase();

                //取得資料
                Utility.GetCarPurposeTypesList(ddlPurposeTypes);

                //再開啟RFID
                rfidscanner.EnableReader();

                NextFocus(tb_eTagEPC);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                MessageBox.Show(ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }

        }

        private void RFIDBinding_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                rfidscanner.DisableReader();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

        }

        private void btnBackTo_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyValue.Equals(134) || e.KeyCode.Equals(Keys.Return))
                {
                    if (String.IsNullOrEmpty(((TextBox)sender).Text))
                    {
                        this.NextFocus(((TextBox)sender));
                        return;
                    }
                    else if ((((TextBox)sender).Text.Trim().ToUpper().IndexOf('-') > -1))
                    {
                        this.NextFocus(ddlPurposeTypes);
                        return;
                    }
                    else
                    {
                        this.tbCarId.Focus();
                        MessageBox.Show("格式不是車號!請重新輸入", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                MessageBox.Show(ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }

        }

        private void NextFocus(Control TextBox)
        {
            try
            {
                switch (TextBox.Name)
                {
                    case "tb_eTagEPC":
                        tb_eTagEPC.Text = "";
                        tb_eTagEPC.ReadOnly = true;
                        tb_eTagEPC.Enabled = false;
                        tb_eTagEPC.Focus();
                        tb_eTagEPC.Text = "";
                        tbCarId.ReadOnly = true;
                        tbCarId.Enabled = false;
                        ddlPurposeTypes.Enabled = false;
                        btnSave.Enabled = false;
                        break;
                    case "tbCarId":
                        tb_eTagEPC.ReadOnly = true;
                        tb_eTagEPC.Enabled = false;

                        this.tbCarId.ReadOnly = false;
                        this.tbCarId.Enabled = true;
                        this.tbCarId.Text = String.Empty;
                        this.tbCarId.BackColor = Color.FromArgb(255, 255, 192);
                        this.tbCarId.Focus();
                        this.ddlPurposeTypes.Enabled = false; ;
                        this.btnSave.Enabled = false;
                        break;
                    case "ddlPurposeTypes":
                        tb_eTagEPC.ReadOnly = true;
                        tb_eTagEPC.Enabled = false;
                        this.tbCarId.ReadOnly = true;
                        this.tbCarId.Enabled = false;
                        this.ddlPurposeTypes.Enabled = true;
                        btnSave.Enabled = true;
                        btnSave.Focus();
                        break;
                    default:
                        TextBox.Enabled = true;
                        TextBox.Focus();
                        break;
                }
                this.Refresh();
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

        }

        private void ddlPurposeTypes_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyValue.Equals(134) || e.KeyCode.Equals(Keys.Return))
                {
                    if (string.IsNullOrEmpty(tb_eTagEPC.Text))
                    {
                        MessageBox.Show("請先掃描eTag!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                        return;
                    }
                    if (string.IsNullOrEmpty(tbCarId.Text))
                    {
                        MessageBox.Show("車號不能為空!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                MessageBox.Show(ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {


            try
            {
                CarPurposeTypes selectedvaile = (CarPurposeTypes)ddlPurposeTypes.SelectedItem;

                if (string.IsNullOrEmpty(tb_eTagEPC.Text))
                {
                    MessageBox.Show("請先掃描eTag!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                    return;
                }
                if (string.IsNullOrEmpty(tbCarId.Text))
                {
                    MessageBox.Show("車號不能為空!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                    return;
                }
                logger.Info(string.Format("EPC={0},CarNumber={1},PurposeTypes={2}", tb_eTagEPC.Text, tbCarId.Text, selectedvaile.Name));
                Utility.SaveETCTagBinding(tb_eTagEPC.Text, tbCarId.Text, selectedvaile.Id);               

                if (MessageBox.Show(string.Format("'{0}' 與車號 '{1}({2})' 的綁定資料儲存成功!", tb_eTagEPC.Text, tbCarId.Text, selectedvaile.Name),
    "系統訊息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {

                    NextFocus(tb_eTagEPC);
                }
                                                
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                MessageBox.Show(ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
        }

    }
}
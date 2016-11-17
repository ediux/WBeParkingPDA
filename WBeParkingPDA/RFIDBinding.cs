using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TCPark_PDA;

namespace WBeParkingPDA
{
    public partial class RFIDBinding : BaseForm
    {
        static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(RFIDBinding));

        private RFIDScanner rfidscanner;
        private SyncDataViewModel memDb;

        private static string EPCID;
        private static string memCarID;
        private static int carpropuseid;

        private bool isDataExists = false;

        public RFIDBinding()
        {
            try
            {

                InitializeComponent();

                EPCID = "";
                memCarID = "";
                carpropuseid = 0;

                rfidscanner = new RFIDScanner(this);
                rfidscanner.TagInputBox = tb_eTagEPC;
                rfidscanner.OnAfterTagRead += new EventHandler<ThingMagic.TagReadDataEventArgs>(rfidscanner_OnAfterTagRead);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }
        private bool isInInactive = true;
        void rfidscanner_OnAfterTagRead(object sender, ThingMagic.TagReadDataEventArgs e)
        {
            try
            {

                isDataExists = false;

                if (isInInactive)
                {
                    isInInactive = false;

                    EPCID = tb_eTagEPC.Text;

                    if (memDb.IsETCExists(EPCID, out memCarID, out carpropuseid))
                    {
                        tbCarId.Text = memCarID;

                        ddlPurposeTypes.SelectedItem = memDb.CarPurposeTypes.First(s => s.Id == carpropuseid);
                        //isDataExists = true;
                        DialogResult dS = Utility.ShowInfoMsg("資料已存在!");
                        if (dS == DialogResult.OK || dS == DialogResult.None)
                        {
                            // isshow = false;
                            //MessageBox.Show("show");

                            //EPCID = "";
                            //memCarID = "";
                            //ddlPurposeTypes.SelectedIndex = 0;
                            //tbCarId.Text = "";
                            //tb_eTagEPC.Text = "";

                            tb_eTagEPC.ReadOnly = true;
                            tb_eTagEPC.Enabled = false;
                            //tb_eTagEPC.Focus();


                            tbCarId.ReadOnly = false;
                            tbCarId.Enabled = true;

                            //ddlPurposeTypes.SelectedIndex = 0;
                            ddlPurposeTypes.Enabled = true;
                            btnSave.Enabled = true;

                            //System.Threading.Thread.Sleep(3000);

                            isDataExists = false;
                            isInInactive = true;


                            //MTRFIDEnable();
                        }

                        return;
                        // MTRFIDDisable();
                    }

                    NextFocus(tbCarId);
                }

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
                memDb = SyncDataViewModel.LoadFile(Utility.jsondbpath);
                //Utility.Initializedatabase();

                //取得資料
                memDb.GetCarPurposeTypesList(ddlPurposeTypes);

                //再開啟RFID
                rfidscanner.EnableReader();

                NextFocus(tb_eTagEPC);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                Utility.ShowErrMsg(ex.Message);
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
            finally
            {
                SyncDataViewModel.SaveFile(Utility.jsondbpath, memDb);
                memDb = null;
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

        protected delegate void voidinvoker();

        private void MTRFIDDisable()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new voidinvoker(MTRFIDDisable));
            }
            else
            {

                rfidscanner.OnAfterTagRead -= rfidscanner_OnAfterTagRead;
                rfidscanner.DisableReader();
                rfidscanner = null;
            }
        }

        protected void MTRFIDEnable()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new voidinvoker(MTRFIDEnable));
            }
            else
            {
                rfidscanner = new RFIDScanner(this);
                rfidscanner.TagInputBox = tb_eTagEPC;
                rfidscanner.OnAfterTagRead += new EventHandler<ThingMagic.TagReadDataEventArgs>(rfidscanner_OnAfterTagRead);
                rfidscanner.EnableReader();
            }
        }

        private void NextFocus(Control TextBox)
        {
            try
            {

                switch (TextBox.Name)
                {
                    case "tb_eTagEPC":
                        tbCarId.Text = "";
                        tb_eTagEPC.Text = "";

                        tb_eTagEPC.ReadOnly = true;
                        tb_eTagEPC.Enabled = false;
                        tb_eTagEPC.Focus();


                        tbCarId.ReadOnly = true;
                        tbCarId.Enabled = false;

                        ddlPurposeTypes.SelectedIndex = 0;
                        ddlPurposeTypes.Enabled = false;
                        btnSave.Enabled = false;
                        break;
                    case "tbCarId":

                        tb_eTagEPC.ReadOnly = true;
                        tb_eTagEPC.Enabled = false;
                        this.ddlPurposeTypes.Enabled = false; ;
                        this.btnSave.Enabled = false;
                        this.tbCarId.ReadOnly = false;
                        this.tbCarId.Enabled = true;
                        this.tbCarId.BackColor = Color.FromArgb(255, 255, 192);
                        this.tbCarId.Focus();

                        break;
                    case "ddlPurposeTypes":
                        tb_eTagEPC.ReadOnly = true;
                        tb_eTagEPC.Enabled = false;
                        tbCarId.ReadOnly = true;
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
                        Utility.ShowErrMsg("請先掃描eTag!");
                        return;
                    }
                    if (string.IsNullOrEmpty(tbCarId.Text))
                    {
                        Utility.ShowErrMsg("車號不能為空!");
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                Utility.ShowErrMsg(ex.Message);
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {


            try
            {
                isDataExists = false;
                //isshow = false;

                CarPurposeTypes selectedvaile = (CarPurposeTypes)ddlPurposeTypes.SelectedItem;
                carpropuseid = selectedvaile.Id;

                if (string.IsNullOrEmpty(tb_eTagEPC.Text))
                {
                    Utility.ShowErrMsg("請先掃描eTag!");
                    return;
                }
                if (string.IsNullOrEmpty(tbCarId.Text))
                {
                    Utility.ShowErrMsg("車號不能為空!");
                    return;
                }

                logger.Info(string.Format("EPC={0},CarNumber={1},PurposeTypes={2}", EPCID, memCarID, selectedvaile.Name));
                memDb.SaveETCTagBinding(EPCID, memCarID, carpropuseid);
                SyncDataViewModel.SaveFile(Utility.jsondbpath, memDb);


                if (Utility.ShowInfoMsg(string.Format("'{0}' 與車號 '{1}({2})' 的綁定資料儲存成功!", EPCID, memCarID, selectedvaile.Name)) == DialogResult.OK)
                {

                    memCarID = "";
                    EPCID = "";
                    tbCarId.Text = "";
                    tb_eTagEPC.Text = "";
                    NextFocus(tb_eTagEPC);
                    isInInactive = true;
                }


                //MTRFIDEnable();

            }
            catch (Exception ex)
            {
                NextFocus(btnSave);
                logger.Error(ex.Message, ex);
                Utility.ShowErrMsg(ex.Message);
            }
        }
        //private bool isshow = false;
        private void tbCarId_GotFocus(object sender, EventArgs e)
        {
            //if (isDataExists)
            //{


            //    //if (isshow == false)
            //    //{
            //    //    isshow = true;
            //    isInInactive = true;

            //    //}
            //    //Utility.ShowInfoMsg("此資料已存在!");
            //}
            //else
            //{
            ShowCarNoKeyBoard(' ');
            //}
        }

        private void ShowCarNoKeyBoard(char inputChar)
        {
            //GBL.Scanner.Off();
            tbCarId.GotFocus -= new EventHandler(tbCarId_GotFocus);

            bool bIsCancel;

            string strTmpStrCar = string.Empty;

            if (inputChar == ' ')
                strTmpStrCar = Utility.GetStringByKeyBoard(string.Empty, KeyBoardInputType.CarNoWithOutCheck, out bIsCancel);
            else
                strTmpStrCar = Utility.GetStringByKeyBoard(tbCarId.Text, KeyBoardInputType.CarNoWithOutCheck, inputChar, out bIsCancel);

            if (bIsCancel == false)
            {
                if (tbCarId.Text == string.Empty)
                {
                    memCarID = strTmpStrCar;
                    tbCarId.Text = strTmpStrCar;
                }
                else if (strTmpStrCar != string.Empty)
                {
                    memCarID = strTmpStrCar;
                    tbCarId.Text = strTmpStrCar;
                }
                //memDb.SaveETCTagBinding(tb_eTagEPC.Text, tbCarId.Text, 0);

                if (memDb.ETCBinding.Any(w => w.CarID.Equals(tbCarId.Text, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Utility.ShowErrMsg(string.Format("車號:{0}\n用途:{1}\n此資料已存在!", tb_eTagEPC.Text, ((CarPurposeTypes)ddlPurposeTypes.SelectedItem).Name));
                    NextFocus(tb_eTagEPC);
                }
                else
                {
                    NextFocus(ddlPurposeTypes);
                }
            }
            else
            {
                EPCID = "";
                memCarID = "";
                ddlPurposeTypes.SelectedIndex = 0;
                tbCarId.Text = "";
                tb_eTagEPC.Text = "";

                tb_eTagEPC.ReadOnly = true;
                tb_eTagEPC.Enabled = false;
                tb_eTagEPC.Focus();


                tbCarId.ReadOnly = true;
                tbCarId.Enabled = false;

                ddlPurposeTypes.SelectedIndex = 0;
                ddlPurposeTypes.Enabled = false;
                btnSave.Enabled = false;

                //System.Threading.Thread.Sleep(3000);

                isDataExists = false;
                isInInactive = true;

                NextFocus(tb_eTagEPC);
            }


            tbCarId.GotFocus += new EventHandler(tbCarId_GotFocus);

        }




    }
}
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
        public class TempVarInMemory
        {
            public string ePCID;
            public string carNumber;
            public int carproposeid;
            public bool CanResponsing;

            public TempVarInMemory()
            {
                ePCID = string.Empty;
                carNumber = string.Empty;
                carproposeid = 0;
                CanResponsing = true;
            }

            public void Reset()
            {
                ePCID = string.Empty;
                carNumber = string.Empty;
                carproposeid = 0;
            }
        }

        static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(RFIDBinding));

#if DEBUG
        //private FakeRFIDScanner rfidscanner;
        private RFIDScanner rfidscanner;
#else
        private RFIDScanner rfidscanner;
#endif

        private SyncDataViewModel memDb;

        private TempVarInMemory MemoryStorage;

        protected delegate void voidinvoker();
        protected delegate void OnTagReadException(object sender, ThingMagic.ReaderExceptionEventArgs e);
        protected delegate void ShowCarNoKeyBoardInvoker(char inputChar);

        #region RFID掃描視窗建構式

        public RFIDBinding()
        {
            try
            {

                InitializeComponent();

                MemoryStorage = new TempVarInMemory();

                //先初始化資料庫
                memDb = SyncDataViewModel.LoadFile(Utility.jsondbpath);
            }
            catch (Exception ex)
            {
                Utility.ShowErrMsg(ex.Message);
                logger.Error(ex.Message, ex);
                this.Close();
            }
        }
        #endregion

        #region Windows Form Events
        private void RFIDBinding_Load(object sender, EventArgs e)
        {
            try
            {
                this.WindowState = FormWindowState.Maximized;

                //取得資料
                memDb.GetCarPurposeTypesList(ddlPurposeTypes);

                //再開啟RFID
                MTRFIDEnable();

                Cursor.Current = Cursors.Default;
                NextFocus(tb_eTagEPC);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                Utility.ShowErrMsg(ex.Message);
                this.Close();
            }

        }

        private void RFIDBinding_Closing(object sender, CancelEventArgs e)
        {
            try
            {

                MTRFIDDisable();
                memDb = null;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void btnBackTo_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
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

                //isshow = false;

                CarPurposeTypes selectedvaile = (CarPurposeTypes)ddlPurposeTypes.SelectedItem;
                if (selectedvaile == null)
                {
                    if (ddlPurposeTypes.Items.Count > 0)
                    {
                        ddlPurposeTypes.SelectedIndex = 0;
                        selectedvaile = (CarPurposeTypes)ddlPurposeTypes.Items[0];
                        MemoryStorage.carproposeid = selectedvaile.Id;
                    }
                }
                else
                {

                    MemoryStorage.carproposeid = selectedvaile.Id;
                }
                if (string.IsNullOrEmpty(MemoryStorage.ePCID))
                {
                    Utility.ShowErrMsg("請先掃描eTag!");
                    return;
                }
                if (string.IsNullOrEmpty(MemoryStorage.carNumber))
                {
                    Utility.ShowErrMsg("車號不能為空!");
                    return;
                }

                logger.Info(string.Format("EPC={0},CarNumber={1},PurposeTypes={2}", MemoryStorage.ePCID, MemoryStorage.carNumber, selectedvaile.Name));

                List<ETCBinding> RemoveAll = memDb.ETCBinding.Where(w => w.ETCID == MemoryStorage.ePCID).ToList();

                if (RemoveAll.Count > 0)
                {
                    foreach (ETCBinding removedata in RemoveAll)
                    {
                        memDb.ETCBinding.Remove(removedata);
                    }
                }
                memDb.ETCBinding.Add(new ETCBinding()
                {
                    CarID = MemoryStorage.carNumber,
                    CarPurposeTypeID = MemoryStorage.carproposeid,
                    CreateTime = DateTime.Now,
                    ETCID = MemoryStorage.ePCID,
                    LastUploadTime = null,
                    LastUpdateTiem = DateTime.Now
                });

                SaveDB();

                if (Utility.ShowInfoMsg(string.Format("'{0}' 與車號 '{1}({2})' 的綁定資料儲存成功!", MemoryStorage.ePCID, MemoryStorage.carNumber, selectedvaile.Name)) == DialogResult.OK)
                {
                    MemoryStorage.Reset();

                    ClearInputBoxs();

                    SetCanInProcessing();
                    SeteTagInputFocus();

                }
            }
            catch (Exception ex)
            {
                NextFocus(btnSave);
                logger.Error(ex.Message, ex);
                //Utility.ShowErrMsg(ex.Message);
                this.Close();
            }
        }



        private void tbCarId_GotFocus(object sender, EventArgs e)
        {
            try
            {
                ShowCarNoKeyBoard(' ');
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                Utility.ShowErrMsg(ex.Message);
                this.Close();
            }

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
                this.Close();
            }

        }

        #endregion

        #region RFID Events

        void rfidscanner_OnAfterTagRead(object sender, ThingMagic.TagReadDataEventArgs e)
        {
            try
            {
                if (MemoryStorage.CanResponsing)
                {
                    SetCanNotInProcessing();

                    MemoryStorage.ePCID = tb_eTagEPC.Text;

                    DisabledbtnGoBack();
                    Cursor.Current = Cursors.WaitCursor;

                    if (memDb.IsETCExists(MemoryStorage.ePCID, out MemoryStorage.carNumber, out MemoryStorage.carproposeid))
                    {
                        Cursor.Current = Cursors.Default;

                        tbCarId.Text = MemoryStorage.carNumber;

                        ddlPurposeTypes.SelectedItem = memDb.CarPurposeTypes.FirstOrDefault(s => s.Id == MemoryStorage.carproposeid);
                        //isDataExists = true;

                        DialogResult dS = Utility.ShowInfoMsg("資料已存在!");

                        if (dS == DialogResult.OK || dS == DialogResult.None)
                        {
                            EnabledbtnGoBack();

                            //SetCanInProcessing();
                            SetCanNotInProcessing();
                            //SeteTagInputFocus();
                            //SetddlPurposeTypes();
                            SetCanEditCarNumber();
                            SetCanInProcessing();
                            return;
                        }
                    }
                    Cursor.Current = Cursors.Default;
                    EnabledbtnGoBack();
                    NextFocus(tbCarId);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                Utility.ShowErrMsg(ex.Message);
                this.Close();   //關閉這個畫面
            }

        }

        private void SetCanInProcessing()
        {
            MemoryStorage.CanResponsing = true;
        }



        void rfidscanner_OnTagReadException(object sender, ThingMagic.ReaderExceptionEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new OnTagReadException(rfidscanner_OnTagReadException), sender, e);
            }
            else
            {
                btnBackTo.Enabled = false;
                Utility.ShowErrMsg(e.ReaderException.Message);
                btnBackTo.Enabled = true;
            }
        }

        #endregion

        #region Helper Function
        private void ClearInputBoxs()
        {
            tbCarId.Text = "";
            tb_eTagEPC.Text = "";
        }

        private void SetCanNotInProcessing()
        {
            MemoryStorage.CanResponsing = false;
        }

        private void SaveDB()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new voidinvoker(SaveDB));
            }
            else
            {
                try
                {
                    SyncDataViewModel.SaveFile(Utility.jsondbpath, memDb);
                }
                catch (Exception)
                {

                    throw;
                }

            }
        }

        private void MTRFIDDisable()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new voidinvoker(MTRFIDDisable));
            }
            else
            {
                try
                {
                    rfidscanner.OnAfterTagRead -= rfidscanner_OnAfterTagRead;
                    rfidscanner.DisableReader();
                    rfidscanner = null;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                    Utility.ShowErrMsg(ex.Message);
                    throw ex;
                }

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
                try
                {
#if DEBUG
                    //rfidscanner = new FakeRFIDScanner(this);
                    rfidscanner = new RFIDScanner(this);
#else
                    rfidscanner = new RFIDScanner(this);
#endif

                    rfidscanner.TagInputBox = tb_eTagEPC;
                    rfidscanner.OnAfterTagRead += new EventHandler<ThingMagic.TagReadDataEventArgs>(rfidscanner_OnAfterTagRead);
                    rfidscanner.OnTagReadException += new EventHandler<ThingMagic.ReaderExceptionEventArgs>(rfidscanner_OnTagReadException);

                    rfidscanner.EnableReader();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                    Utility.ShowErrMsg(ex.Message);
                    this.Close();
                }

            }
        }
        private void NextFocus(Control TextBox)
        {
            try
            {

                switch (TextBox.Name)
                {
                    case "tb_eTagEPC":
                        SeteTagInputFocus();
                        break;
                    case "tbCarId":
                        SetCarNumberFocus();
                        break;
                    case "ddlPurposeTypes":
                        SetddlPurposeTypes();
                        break;
                    default:
                        TextBox.Enabled = true;
                        TextBox.Focus();
                        break;
                }
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                //Utility.ShowErrMsg(ex.Message);
                this.Close();
            }

        }

        private void EnabledbtnGoBack()
        {
            btnBackTo.Enabled = true;
        }

        private void DisabledbtnGoBack()
        {
            btnBackTo.Enabled = false;
        }

        private void SetCanEditCarNumber()
        {


            tb_eTagEPC.ReadOnly = true;
            tb_eTagEPC.Enabled = false;

            tbCarId.ReadOnly = false;
            tbCarId.Enabled = true;

            //ddlPurposeTypes.SelectedIndex = 0;
            ddlPurposeTypes.Enabled = true;

            btnSave.Enabled = true;

            ddlPurposeTypes.Focus();
        }

        private void SeteTagInputFocus()
        {
            ClearInputBoxs();

            tb_eTagEPC.ReadOnly = true;
            tb_eTagEPC.Enabled = false;

            tbCarId.ReadOnly = true;
            tbCarId.Enabled = false;

            ddlPurposeTypes.SelectedIndex = 0;
            ddlPurposeTypes.Enabled = false;

            btnSave.Enabled = false;

            tb_eTagEPC.Focus();
        }

        private void SetCarNumberFocus()
        {
            this.tb_eTagEPC.ReadOnly = true;
            this.tb_eTagEPC.Enabled = false;


            this.tbCarId.ReadOnly = false;
            this.tbCarId.Enabled = true;
            this.tbCarId.BackColor = Color.FromArgb(255, 255, 192);

            this.ddlPurposeTypes.Enabled = false; ;
            this.btnSave.Enabled = false;

            this.tbCarId.Focus();
        }

        private void SetddlPurposeTypes()
        {
            tb_eTagEPC.ReadOnly = true;
            tb_eTagEPC.Enabled = false;

            tbCarId.ReadOnly = true;
            tbCarId.Enabled = false;

            ddlPurposeTypes.Enabled = true;

            btnSave.Enabled = true;
            btnSave.Focus();
        }


        private void ShowCarNoKeyBoard(char inputChar)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ShowCarNoKeyBoardInvoker(ShowCarNoKeyBoard), inputChar);
            }
            else
            {
                try
                {
                    SetCanNotInProcessing();
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

                        //memDb.SaveETCTagBinding(tb_eTagEPC.Text, tbCarId.Text, 0);

                        //if (memDb.ETCBinding.Any(w => w.CarID.Equals(strTmpStrCar, StringComparison.InvariantCultureIgnoreCase)))
                        //{
                        //    if (Utility.ShowInfoMsg(string.Format("車號:{0}\n用途:{1}\n此資料已存在!", strTmpStrCar, ((CarPurposeTypes)ddlPurposeTypes.SelectedItem).Name)) == DialogResult.OK)
                        //    {
                        //        tbCarId.Text = strTmpStrCar;
                        //        SetCarNumberFocus();
                        //        SetCanNotInProcessing();
                        //    }

                        //}
                        //else
                        //{
                        if (strTmpStrCar == string.Empty)
                        {
                            MemoryStorage.carNumber = strTmpStrCar;
                            tbCarId.Text = strTmpStrCar;
                            SeteTagInputFocus();
                            SetCanInProcessing();
                        }
                        else if (strTmpStrCar != string.Empty)
                        {
                            MemoryStorage.carNumber = strTmpStrCar;
                            tbCarId.Text = strTmpStrCar;
                            MemoryStorage.carNumber = strTmpStrCar;
                            SetddlPurposeTypes();
                            SetCanNotInProcessing();
                        }

                        //}


                    }
                    else
                    {
                        MemoryStorage.Reset();
                        ddlPurposeTypes.SelectedIndex = 0;
                        EnabledbtnGoBack();
                        SetCanInProcessing();
                        SeteTagInputFocus();
                    }


                    tbCarId.GotFocus += new EventHandler(tbCarId_GotFocus);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    Utility.ShowErrMsg(ex.Message);
                    this.Close();
                }
            }


        }
        #endregion

        private void tbCarId_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                ShowCarNoKeyBoard(' ');
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                Utility.ShowErrMsg(ex.Message);
                this.Close();
            }
        }

    }
}